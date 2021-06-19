﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Forms;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Abstractions.Repositories;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Abstractions.Setup;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Extensions;
using RapidCMS.Core.Models.EventArgs.Mediators;
using RapidCMS.ModelMaker.Abstractions.CommandHandlers;
using RapidCMS.ModelMaker.Abstractions.Config;
using RapidCMS.ModelMaker.Core.Abstractions.Validation;
using RapidCMS.ModelMaker.EqualityComparers;
using RapidCMS.ModelMaker.Models.Commands;
using RapidCMS.ModelMaker.Models.Entities;
using RapidCMS.ModelMaker.Models.Responses;

namespace RapidCMS.ModelMaker.Repositories
{
    internal class PropertyRepository : IRepository
    {
        private readonly IModelMakerConfig _config;
        private readonly ICommandHandler<UpdateRequest<ModelEntity>, ConfirmResponse> _updateEntityCommandHandler;
        private readonly ISetupResolver<ICollectionSetup> _collectionSetupResolver;
        private readonly IMediator _mediator;

        public PropertyRepository(
            IModelMakerConfig config,
            ICommandHandler<UpdateRequest<ModelEntity>, ConfirmResponse> updateEntityCommandHandler,
            ISetupResolver<ICollectionSetup> collectionSetupResolver,
            IMediator mediator)
        {
            _config = config;
            _updateEntityCommandHandler = updateEntityCommandHandler;
            _collectionSetupResolver = collectionSetupResolver;
            _mediator = mediator;
        }

        public Task AddAsync(IRelatedViewContext viewContext, string id) => throw new NotSupportedException();

        public async Task DeleteAsync(string id, IViewContext viewContext)
        {
            if (viewContext.Parent?.Entity is ModelEntity model)
            {
                model.Properties.RemoveAll(x => x.Id == id);

                await _updateEntityCommandHandler.HandleAsync(new UpdateRequest<ModelEntity>(model));
            }
        }

        public Task<IEnumerable<IEntity>> GetAllAsync(IViewContext viewContext, IQuery query)
        {
            if (viewContext.Parent?.Entity is ModelEntity model)
            {
                return Task.FromResult<IEnumerable<IEntity>>(model.Properties);
            }

            throw new InvalidOperationException();
        }

        public Task<IEnumerable<IEntity>> GetAllNonRelatedAsync(IRelatedViewContext viewContext, IQuery query) => throw new NotSupportedException();

        public Task<IEnumerable<IEntity>> GetAllRelatedAsync(IRelatedViewContext viewContext, IQuery query) => throw new NotSupportedException();

        public Task<IEntity?> GetByIdAsync(string id, IViewContext viewContext)
        {
            if (viewContext.Parent?.Entity is ModelEntity model &&
                model.Properties.FirstOrDefault(x => x.Id == id) is PropertyModel property)
            {
                var allPropertyValidations = _config.Properties
                    .Single(prop => prop.Alias == property.PropertyAlias)
                    .Validators
                    .Select(validation =>
                    {
                        var config = Activator.CreateInstance(validation.Config) as IValidatorConfig;

                        var validationModel = Activator.CreateInstance(typeof(PropertyValidationModel<>).MakeGenericType(validation.Config)) as PropertyValidationModel
                            ?? throw new InvalidOperationException("Could not create correct PropertyValidationModel.");

                        validationModel.Alias = validation.Alias;
                        validationModel.Config = config;
                        validationModel.Id = Guid.NewGuid().ToString();

                        return validationModel;
                    })
                    .ToList();

                property.Validations = property.Validations.Union(allPropertyValidations, new PropertyValidationModelEqualityComparer()).ToList();

                return Task.FromResult<IEntity?>(property);
            }

            return Task.FromResult<IEntity?>(default);
        }

        public async Task<IEntity?> InsertAsync(IEditContext editContext)
        {
            if (editContext is IEditContext<PropertyModel> typedEditContext &&
                typedEditContext.Parent?.Entity is ModelEntity model)
            {
                var newProperty = typedEditContext.Entity;
                newProperty.Id = Guid.NewGuid().ToString();
                model.Properties.Add(newProperty);

                await SavePropertyConfigsAsync(model, typedEditContext.Entity);

                if (newProperty.IsTitle)
                {
                    SetPropertyAsOnlyTitle(model, newProperty);
                }

                ValidateProperty(typedEditContext, newProperty, model);

                typedEditContext.EnforceValidEntity();

                await _updateEntityCommandHandler.HandleAsync(new UpdateRequest<ModelEntity>(model));

                _mediator.NotifyEvent(this, new RepositoryEventArgs(typeof(ModelRepository), default, model.Id, CrudType.Insert));

                return newProperty;
            }

            return default;
        }

        public Task<IEntity> NewAsync(IViewContext viewContext, Type? variantType)
        {
            return Task.FromResult<IEntity>(new PropertyModel
            {
                Validations = _config.Properties
                    .SelectMany(validation => validation.Validators)
                    .GroupBy(validation => validation.Alias)
                    .Select(validation => validation.First())
                    .Select(validation =>
                    {
                        var config = Activator.CreateInstance(validation.Config) as IValidatorConfig;

                        var validationModel = Activator.CreateInstance(typeof(PropertyValidationModel<>).MakeGenericType(validation.Config)) as PropertyValidationModel
                            ?? throw new InvalidOperationException("Could not create correct PropertyValidationModel.");

                        validationModel.Alias = validation.Alias;
                        validationModel.Config = config;
                        validationModel.Id = Guid.NewGuid().ToString();

                        return validationModel;
                    })
                    .ToList()
            });
        }

        public Task RemoveAsync(IRelatedViewContext viewContext, string id) => throw new NotSupportedException();

        public async Task ReorderAsync(string? beforeId, string id, IViewContext viewContext)
        {
            if (viewContext.Parent?.Entity is ModelEntity model)
            {
                var property = model.Properties.FirstOrDefault(x => x.Id == id);
                if (property == null)
                {
                    return;
                }

                model.Properties.Remove(property);

                var targetIndex = model.Properties.FindIndex(x => x.Id == beforeId);
                if (targetIndex == -1)
                {
                    model.Properties.Add(property);
                }
                else
                {
                    model.Properties.Insert(targetIndex, property);
                }

                await _updateEntityCommandHandler.HandleAsync(new UpdateRequest<ModelEntity>(model));

                // TODO: this does not update its parent entity so when updating that one, the old order is restored
                // if a mediator update is issued then multiple reorders get messed up as the refresh takes place after first update, while others should also be updated..
            }
        }

        public async Task UpdateAsync(IEditContext editContext)
        {
            if (editContext is IEditContext<PropertyModel> typedEditContext &&
                typedEditContext.Parent?.Entity is ModelEntity model)
            {
                var index = model.Properties.FindIndex(x => x.Id == typedEditContext.Entity.Id);

                model.Properties[index] = typedEditContext.Entity;
                
                await SavePropertyConfigsAsync(model, typedEditContext.Entity);
                if (typedEditContext.Entity.IsTitle)
                {
                    SetPropertyAsOnlyTitle(model, typedEditContext.Entity);
                }

                ValidateProperty(typedEditContext, typedEditContext.Entity, model);

                typedEditContext.EnforceValidEntity();

                await _updateEntityCommandHandler.HandleAsync(new UpdateRequest<ModelEntity>(model));

                _mediator.NotifyEvent(this, new RepositoryEventArgs(typeof(ModelRepository), default, model.Id, CrudType.Update));
            }
        }

        private static void ValidateProperty(IEditContext<PropertyModel> editContext, PropertyModel property, ModelEntity model)
        {
            if (model.Properties.Count(x => x.Name == property.Name) > 1)
            {
                editContext.AddValidationError("Alias", "Alias already used.");
            }
        }

        private async Task SavePropertyConfigsAsync(ModelEntity model, PropertyModel property)
        {
            var propertyConfig = _config.Properties.FirstOrDefault(x => x.Alias == property.PropertyAlias);
            if (propertyConfig != null)
            {
                if (!propertyConfig.UsableAsTitle)
                {
                    property.IsTitle = false;
                }

                var editorConfig = _config.Editors.FirstOrDefault(x => x.Alias == property.EditorAlias);

                property.Type = propertyConfig.Type.FullName;
                property.IsRelationToOne = propertyConfig.IsRelationToOne;
                property.IsRelationToMany = propertyConfig.IsRelationToMany;
                property.EditorType = editorConfig?.Editor.FullName;

                var validations = _config.Validators.Where(x => propertyConfig.Validators.Any(v => v.Alias == x.Alias));

                var newValidations = new List<PropertyValidationModel>();

                foreach (var validation in validations)
                {
                    var config = property.Validations.FirstOrDefault(x => x.Alias == validation.Alias)?.Config
                        ?? Activator.CreateInstance(validation.Config) as IValidatorConfig;

                    if (config?.IsApplicable(property) == true || config?.AlwaysIncluded == true)
                    {
                        var validationModel = Activator.CreateInstance(typeof(PropertyValidationModel<>).MakeGenericType(validation.Config)) as PropertyValidationModel
                            ?? throw new InvalidOperationException("Could not create correct PropertyValidationModel.");

                        validationModel.Alias = validation.Alias;
                        validationModel.Config = config;
                        validationModel.Id = Guid.NewGuid().ToString();

                        validationModel.AttributeExpression = config?.ValidationAttributeExpression;
                        validationModel.DataCollectionExpression = config?.DataCollectionExpression;

                        newValidations.Add(validationModel);
                    }
                }

                try
                {
                    var relatedCollectionAlias = newValidations.Select(x => x.Config?.RelatedCollectionAlias).OfType<string>().SingleOrDefault();

                    if (!string.IsNullOrWhiteSpace(relatedCollectionAlias))
                    {
                        var relatedCollectionEntityVariant = (await _collectionSetupResolver.ResolveSetupAsync(relatedCollectionAlias)).EntityVariant;

                        property.Type = relatedCollectionEntityVariant.Type.FullName;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException("Cannot have 2 validation configs with RelatedCollectionAlias", ex);
                }


                property.Validations = newValidations;
            }
        }

        private static void SetPropertyAsOnlyTitle(ModelEntity model, PropertyModel property)
        {
            model.Properties.Where(x => x.Name != property.Name).ForEach(x => x.IsTitle = false);
        }
    }
}