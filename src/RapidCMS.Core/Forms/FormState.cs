﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Forms;
using RapidCMS.Core.Abstractions.Metadata;
using RapidCMS.Core.Attributes;
using RapidCMS.Core.Extensions;
using RapidCMS.Core.Helpers;
using RapidCMS.Core.Models.Setup;
using RapidCMS.Core.Validators;

namespace RapidCMS.Core.Forms
{
    internal class FormState
    {
        private readonly List<string> _messages = new List<string>();
        private readonly List<PropertyState> _fieldStates = new List<PropertyState>();
        private readonly IEntity _entity;
        private readonly IReadOnlyList<ValidationSetup> _validators;

        public FormState(IEntity entity, IReadOnlyList<ValidationSetup> validators, IServiceProvider serviceProvider)
        {
            _entity = entity;
            _validators = validators;
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public IEnumerable<string> GetValidationMessages()
        {
            foreach (var message in _messages)
            {
                yield return message;
            }
            foreach (var message in _fieldStates.SelectMany(x => x.GetValidationMessages()))
            {
                yield return message;
            }
        }

        public IEnumerable<string> GetStrayValidationMessages()
        {
            return _messages;
        }

        public void AddMessage(string message)
        {
            _messages.Add(message);
        }

        public void ClearMessages()
        {
            _messages.Clear();
            foreach (var fieldState in _fieldStates)
            {
                fieldState.ClearMessages();
            }
        }

        public ModelStateDictionary ModelState
        {
            get
            {
                var state = new ModelStateDictionary();

                _fieldStates.ForEach(fs => fs.GetValidationMessages().ForEach(message => state.Add(fs.Property.PropertyName, message)));
                _messages.ForEach(m => state.Add(string.Empty, m));

                return state;
            }
        }

        public void PopulatePropertyStatesUsingReferenceEntity(IEntity reference)
        {
            GetPropertyMetadatas(reference).ForEach(property =>
            {
                if ((property.PropertyType.IsValueType ||
                    property.PropertyType == typeof(string)) &&
                    !Equals(property.Getter(reference), property.Getter(_entity)))
                {
                    GetPropertyState(property)!.IsModified = true;
                }
            });
        }

        public void PopulateAllPropertyStates()
        {
            GetPropertyMetadatas(_entity).ForEach(property => GetPropertyState(property, createWhenNotFound: true));
        }

        private IEnumerable<IPropertyMetadata> GetPropertyMetadatas(IEntity reference, IEnumerable<PropertyInfo>? objectGetters = default)
        {
            Func<object, object> getObject;
            if (objectGetters == null)
            {
                getObject = (root) => root;
            }
            else
            {
                getObject = (root) => objectGetters.Aggregate(root, (@obj, objectGetter) => objectGetter.GetValue(@obj)!);
            }

            var properties = getObject(reference).GetType().GetProperties();

            foreach (var property in properties)
            {
                var validateObjectAttribute = property.GetCustomAttribute<ValidateObjectAttribute>();
                if (validateObjectAttribute != null)
                {
                    // only venture into nested objects when the model wants them validated
                    foreach (var nestedPropertyMetadata in GetPropertyMetadatas(reference, (objectGetters ?? new PropertyInfo[] { }).Union(new[] { property })))
                    {
                        yield return nestedPropertyMetadata;
                    }
                }

                var propertyMetadata = PropertyMetadataHelper.GetPropertyMetadata(reference.GetType(), objectGetters, property);
                if (propertyMetadata == null)
                {
                    continue;
                }

                yield return propertyMetadata;
            }
        }

        public IEnumerable<PropertyState> GetPropertyStates()
        {
            return _fieldStates;
        }

        public PropertyState? GetPropertyState(string propertyName)
        {
            return _fieldStates.SingleOrDefault(field => field.Property.PropertyName == propertyName);
        }

        public PropertyState? GetPropertyState(IPropertyMetadata property, bool createWhenNotFound = true)
        {
            var fieldState = _fieldStates.SingleOrDefault(x => x.Property.Fingerprint == property.Fingerprint);
            if (fieldState == null)
            {
                if (!createWhenNotFound)
                {
                    return default;
                }

                fieldState = new PropertyState(property);
                _fieldStates.Add(fieldState);
            }

            return fieldState;
        }

        public async Task ValidateModelAsync(IRelationContainer relationContainer)
        {
            var results = await GetValidationResultsForModelAsync(relationContainer);

            foreach (var result in results)
            {
                var strayError = true;
                result.MemberNames.ForEach(name =>
                {
                    GetPropertyState(name)?.AddMessage(result.ErrorMessage ?? "Unknown error");
                    strayError = false;
                });

                if (strayError)
                {
                    _messages.Add(result.ErrorMessage ?? "Unknown error");
                }
            }

            _fieldStates.ForEach(x => x.WasValidated = true);
        }

        public async Task ValidatePropertyAsync(IPropertyMetadata property, IRelationContainer relationContainer)
        {
            var results = await GetValidationResultsForPropertyAsync(property, relationContainer);

            var state = GetPropertyState(property)!;
            state.ClearMessages(clearManualMessages: true);
            state.WasValidated = true;

            foreach (var result in results)
            {
                state.AddMessage(result.ErrorMessage ?? "Unknown error");
            }
        }

        private async Task<IEnumerable<ValidationResult>> GetValidationResultsForPropertyAsync(IPropertyMetadata property, IRelationContainer relationContainer)
        {
            var results = await EntityValidator.ValidateAsync(_validators, relationContainer, ServiceProvider, _entity, property.PropertyName).ToListAsync();
            return results;
        }

        private async Task<IEnumerable<ValidationResult>> GetValidationResultsForModelAsync(IRelationContainer relationContainer)
        {
            var results = await EntityValidator.ValidateAsync(_validators, relationContainer, ServiceProvider, _entity).ToListAsync();

            ClearMessages();

            _fieldStates
                .Where(kv => kv.IsBusy)
                .ForEach(kv => results.Add(new ValidationResult(
                    $"The {kv.Property.PropertyName} field indicates it is performing an asynchronous task which must be awaited.",
                    new[] { kv.Property.PropertyName })));

            return results;
        }
    }
}
