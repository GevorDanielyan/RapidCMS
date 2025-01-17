﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Forms;
using RapidCMS.Core.Converters;
using RapidCMS.Core.Exceptions;
using RapidCMS.Core.Helpers;
using RapidCMS.Core.Models.ApiBridge;
using RapidCMS.Core.Models.ApiBridge.Request;
using RapidCMS.Core.Models.ApiBridge.Response;
using RapidCMS.Core.Repositories;

namespace RapidCMS.Repositories.ApiBridge
{
    public class ApiRepository<TEntity> : BaseRepository<TEntity>
        where TEntity : class, IEntity
    {
        private readonly ApiRepositoryHelper _apiRepositoryHelper;

        public ApiRepository(IHttpClientFactory httpClientFactory)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new EntityModelJsonConverter<TEntity>());
            var repositoryAlias = AliasHelper.GetRepositoryAlias(typeof(ApiRepository<TEntity>));

            _apiRepositoryHelper = new ApiRepositoryHelper(httpClientFactory, jsonSerializerSettings, repositoryAlias);
        }

        public override Task DeleteAsync(string id, IParent? parent)
            => _apiRepositoryHelper.DoRequestAsync(_apiRepositoryHelper.CreateRequest(HttpMethod.Delete, $"entity/{id}", new DeleteModel(parent)));

        public override async Task<IEnumerable<TEntity>> GetAllAsync(IParent? parent, IView<TEntity> view)
        {
            var results = await _apiRepositoryHelper.DoRequestAsync<EntitiesModel<TEntity>>(_apiRepositoryHelper.CreateRequest(HttpMethod.Post, "all", new ParentQueryModel(parent, view)));
            if (results == default)
            {
                return Enumerable.Empty<TEntity>();
            }

            view.HasMoreData(results.MoreDataAvailable);

            return results.Entities.Select(x => x.Entity);
        }

        public override async Task<IEnumerable<TEntity>?> GetAllRelatedAsync(IRelated related, IView<TEntity> view)
        {
            var results = await _apiRepositoryHelper.DoRequestAsync<EntitiesModel<TEntity>>(_apiRepositoryHelper.CreateRequest(HttpMethod.Post, "all/related", new RelatedQueryModel(related, view)));
            if (results == default)
            {
                return Enumerable.Empty<TEntity>();
            }

            view.HasMoreData(results.MoreDataAvailable);

            return results.Entities.Select(x => x.Entity);
        }

        public override async Task<IEnumerable<TEntity>?> GetAllNonRelatedAsync(IRelated related, IView<TEntity> view)
        {
            var results = await _apiRepositoryHelper.DoRequestAsync<EntitiesModel<TEntity>>(_apiRepositoryHelper.CreateRequest(HttpMethod.Post, "all/nonrelated", new RelatedQueryModel(related, view)));
            if (results == default)
            {
                return Enumerable.Empty<TEntity>();
            }

            view.HasMoreData(results.MoreDataAvailable);

            return results.Entities.Select(x => x.Entity);
        }

        public override async Task<TEntity?> GetByIdAsync(string id, IParent? parent)
            => (await _apiRepositoryHelper.DoRequestAsync<EntityModel<TEntity>>(_apiRepositoryHelper.CreateRequest(HttpMethod.Post, $"entity/{id}", new ParentQueryModel(parent))))?.Entity;

        public override async Task<TEntity?> InsertAsync(IEditContext<TEntity> editContext)
            => (await _apiRepositoryHelper.DoRequestAsync<EntityModel<TEntity>>(_apiRepositoryHelper.CreateRequest(HttpMethod.Post, "entity", new EditContextModel<TEntity>(editContext)), editContext))?.Entity;

        public override async Task<TEntity> NewAsync(IParent? parent, Type? variantType = null)
            => (await _apiRepositoryHelper.DoRequestAsync<EntityModel<TEntity>>(_apiRepositoryHelper.CreateRequest(HttpMethod.Post, "new", new ParentQueryModel(parent, variantType))))?.Entity ?? throw new NotFoundException("Could not create new entity.");

        public override Task UpdateAsync(IEditContext<TEntity> editContext)
            => _apiRepositoryHelper.DoRequestAsync(_apiRepositoryHelper.CreateRequest(HttpMethod.Put, $"entity/{editContext.Entity.Id}", new EditContextModel<TEntity>(editContext)), editContext);

        public override Task AddAsync(IRelated related, string id)
            => _apiRepositoryHelper.DoRequestAsync(_apiRepositoryHelper.CreateRequest(HttpMethod.Post, $"relate", new RelateModel(related, id)));

        public override Task RemoveAsync(IRelated related, string id)
            => _apiRepositoryHelper.DoRequestAsync(_apiRepositoryHelper.CreateRequest(HttpMethod.Delete, $"relate", new RelateModel(related, id)));

        public override Task ReorderAsync(string? beforeId, string id, IParent? parent)
            => _apiRepositoryHelper.DoRequestAsync(_apiRepositoryHelper.CreateRequest(HttpMethod.Post, $"reorder", new ReorderModel(beforeId, id, parent)));        
    }
}
