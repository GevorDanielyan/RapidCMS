﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RapidCMS.Core.Abstractions.Navigation;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Abstractions.Services;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Extensions;
using RapidCMS.Core.Forms;
using RapidCMS.Core.Models.Data;
using RapidCMS.Core.Models.Setup;
using RapidCMS.Core.Models.UI;
using RapidCMS.Core.Navigation;
using RapidCMS.Core.Providers;

namespace RapidCMS.Core.Resolvers.UI
{
    internal class BaseUIResolver
    {
        private readonly IDataProviderResolver _dataProviderResolver;
        private readonly IButtonActionHandlerResolver _buttonActionHandlerResolver;
        protected readonly IAuthService _authService;
        private readonly INavigationStateProvider _navigationStateProvider;

        protected BaseUIResolver(
            IDataProviderResolver dataProviderResolver,
            IButtonActionHandlerResolver buttonActionHandlerResolver,
            IAuthService authService,
            INavigationStateProvider navigationStateProvider)
        {
            _dataProviderResolver = dataProviderResolver;
            _buttonActionHandlerResolver = buttonActionHandlerResolver;
            _authService = authService;
            _navigationStateProvider = navigationStateProvider;
        }

        protected async Task<List<ButtonUI>> GetButtonsAsync(IEnumerable<ButtonSetup> buttons, FormEditContext editContext)
        {
            return await buttons
                .GetAllButtons()
                .SelectNotNullAsync(async button =>
                {
                    var handler = _buttonActionHandlerResolver.GetButtonActionHandler(button);
                    if (handler.IsCompatible(button, editContext) && 
                        await _authService.IsUserAuthorizedAsync(editContext, button))
                    {
                        return new ButtonUI(handler, button, editContext);
                    }
                    else
                    {
                        return default;
                    }
                })
                .ToListAsync();
        }

        protected internal async Task<SectionUI> GetSectionUIAsync(PaneSetup pane, FormEditContext editContext, NavigationState navigationState)
        {
            var fields = await pane.Fields.ToListAsync(async field =>
            {
                var dataProvider = await _dataProviderResolver.GetDataProviderAsync(field);
                if (dataProvider != null)
                {
                    editContext.DataProviders.Add(dataProvider);
                }

                return (index: field.Index, element: (ElementUI)GetField(field, dataProvider));
            });

            var subCollections = pane.SubCollectionLists
                .Where(subCollection => subCollection.SupportsUsageType.FindSupportedUsageType(editContext.UsageType) > UsageType.None)
                .Select(subCollection =>
                {
                    var parentPath = ParentPath.AddLevel(editContext.Parent?.GetParentPath(), editContext.RepositoryAlias, editContext.Entity.Id!);

                    // TODO: this does not read back the state (needed when nested states are saved in url)
                    var nestedState = new NavigationState(
                        subCollection.CollectionAlias,
                        parentPath,
                        subCollection.SupportsUsageType.FindSupportedUsageType(editContext.UsageType) | UsageType.List);

                    _navigationStateProvider.NestNavigationState(navigationState, nestedState);

                    return (index: subCollection.Index, element: (ElementUI)new SubCollectionUI(subCollection, nestedState));
                });

            var relatedCollections = pane.RelatedCollectionLists
                .Where(relatedCollection => relatedCollection.SupportsUsageType.FindSupportedUsageType(editContext.UsageType) > UsageType.None)
                .Select(relatedCollection =>
                {
                    var parentPath = ParentPath.AddLevel(editContext.Parent?.GetParentPath(), editContext.RepositoryAlias, editContext.Entity.Id!);

                    // TODO: this does not read back the state (needed when nested states are saved in url)
                    var nestedState = new NavigationState(
                        relatedCollection.CollectionAlias,
                        parentPath,
                        null,
                        new RelatedEntity(editContext),
                        relatedCollection.SupportsUsageType.FindSupportedUsageType(editContext.UsageType) | UsageType.List,
                        PageType.Collection);

                    _navigationStateProvider.NestNavigationState(navigationState, nestedState);

                    return (index: relatedCollection.Index, element: (ElementUI)new RelatedCollectionUI(relatedCollection, nestedState));
                });

            return new SectionUI(pane.IsVisible)
            {
                Buttons = await GetButtonsAsync(pane.Buttons, editContext),
                CustomType = pane.CustomType,
                Label = pane.Label,

                Elements = fields
                    .Union(subCollections)
                    .Union(relatedCollections)
                    .OrderBy(x => x.index)
                    .ToList(x => x.element)
            };
        }

        protected static FieldUI GetField(FieldSetup field, FormDataProvider? dataProvider) 
            => field switch
            {
                CustomExpressionFieldSetup x => new CustomExpressionFieldUI(x),
                ExpressionFieldSetup x => new ExpressionFieldUI(x),

                CustomPropertyFieldSetup x => new CustomPropertyFieldUI(x, dataProvider),
                PropertyFieldSetup x => new PropertyFieldUI(x, dataProvider),

                _ => throw new InvalidOperationException($"Cannot return FieldUI for given field of type {field?.GetType()}")
            };
    }
}
