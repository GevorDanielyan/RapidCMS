﻿using Moq;
using NUnit.Framework;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Dispatchers;
using RapidCMS.Core.Abstractions.Factories;
using RapidCMS.Core.Abstractions.Interactions;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Abstractions.Navigation;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Abstractions.Services;
using RapidCMS.Core.Dispatchers.Form;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Forms;
using RapidCMS.Core.Models.Request.Form;
using RapidCMS.Core.Models.Response;
using RapidCMS.Core.Models.Setup;
using RapidCMS.Core.Services.Concurrency;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RapidCMS.Core.Tests.Services.Dispatchers
{
    public class EntitiesInteractionDispatcherTests
    {
        private IInteractionDispatcher<PersistEntitiesRequestModel, ListViewCommandResponseModel> _subject = default!;

        private Mock<IServiceProvider> _serviceProviderMock = default!;

        private Mock<INavigationStateProvider> _navigationStateProvider = default!;
        private Mock<ISetupResolver<CollectionSetup>> _collectionResolver = default!;
        private CollectionSetup _collection = default!;
        private EntityVariantSetup _entityVariant = default!;
        private Mock<IRepositoryResolver> _repositoryResolver = default!;
        private IConcurrencyService _concurrencyService = default!;
        private Mock<IButtonInteraction> _buttonInteraction = default!;
        private Mock<IEditContextFactory> _editContextFactory = default!;
        private Mock<IMediator> _mediator = default!;

        [SetUp]
        public void Setup()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();

            _navigationStateProvider = new Mock<INavigationStateProvider>();
            _entityVariant = new EntityVariantSetup("ev", "icon", typeof(IEntity), "alias");
            _collection = new CollectionSetup("icon", "color", "name", "alias", "repo")
            {
                EntityVariant = _entityVariant
            };
            _collectionResolver = new Mock<ISetupResolver<CollectionSetup>>();
            _collectionResolver
                .Setup(x => x.ResolveSetupAsync(It.IsAny<string>()))
                .ReturnsAsync(_collection);

            _repositoryResolver = new Mock<IRepositoryResolver>();
            _concurrencyService = new ConcurrencyService(new SemaphoreSlim(1, 1));
            _buttonInteraction = new Mock<IButtonInteraction>();
            _editContextFactory = new Mock<IEditContextFactory>();
            _mediator = new Mock<IMediator>();

            _subject = new EntitiesInteractionDispatcher(
                _navigationStateProvider.Object,
                _collectionResolver.Object,
                _repositoryResolver.Object,
                _concurrencyService,
                _buttonInteraction.Object,
                _editContextFactory.Object,
                _mediator.Object);
        }

        [TestCase("alias1")]
        [TestCase("alias2")]
        public void WhenInvokingInteraction_ThenCollectionShouldBeResolvedUsingGivenCollectionAlias(string alias)
        {
            // arrange
            var request = new PersistEntitiesRequestModel
            {
                ListContext = new ListContext(alias, new FormEditContext(alias, alias, alias, new DefaultEntityVariant(), default, UsageType.Add, new List<ValidationSetup>(), _serviceProviderMock.Object), default, UsageType.Add, default, _serviceProviderMock.Object)
            };

            // act
            _subject.InvokeAsync(request);

            // assert
            _collectionResolver.Verify(x => x.ResolveSetupAsync(It.Is<string>(x => x == alias)));
        }

        [TestCase("alias1")]
        [TestCase("alias2")]
        public void WhenInvokingInteraction_ThenRepositoryShouldBeResolvedByGivenCollection(string alias)
        {
            // arrange
            var request = new PersistEntitiesRequestModel
            {
                ListContext = new ListContext(alias, new FormEditContext(alias, alias, alias, new DefaultEntityVariant(), default, UsageType.Add, new List<ValidationSetup>(), _serviceProviderMock.Object), default, UsageType.Add, default, _serviceProviderMock.Object)
            };

            // act
            _subject.InvokeAsync(request);

            // assert
            _repositoryResolver.Verify(x => x.GetRepository(It.Is<CollectionSetup>(x => x == _collection)));
        }

        [TestCase("alias1")]
        [TestCase("alias2")]
        public void WhenInvokingInteraction_ThenCrudTypeShouldBeDeterminedUsingButtonInteraction(string alias)
        {
            // arrange
            var request = new PersistEntitiesRequestModel
            {
                ListContext = new ListContext(alias, new FormEditContext(alias, alias, alias, new DefaultEntityVariant(), default, UsageType.Add, new List<ValidationSetup>(), _serviceProviderMock.Object), default, UsageType.Add, default, _serviceProviderMock.Object)
            };

            // act
            _subject.InvokeAsync(request);

            // assert
            _buttonInteraction.Verify(x => x.ValidateButtonInteractionAsync(It.Is<IListButtonInteractionRequestModel>(x => x == request)));
        }

        [TestCase("alias1")]
        [TestCase("alias2")]
        public void WhenInvokingInteraction_ThenCompletedInteractionShouldBeFlaggedToButtonInteraction(string alias)
        {
            // arrange
            var request = new PersistEntitiesRequestModel
            {
                ListContext = new ListContext(alias, new FormEditContext(alias, alias, alias, new DefaultEntityVariant(), default, UsageType.Add, new List<ValidationSetup>(), _serviceProviderMock.Object), default, UsageType.Add, default, _serviceProviderMock.Object)
            };

            // act
            _subject.InvokeAsync(request);

            // assert
            _buttonInteraction.Verify(x => x.CompleteButtonInteractionAsync(It.Is<IListButtonInteractionRequestModel>(x => x == request)));
        }
    }
}
