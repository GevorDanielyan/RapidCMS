﻿using System.IO;
using System.Threading.Tasks;
using RapidCMS.ModelMaker.Abstractions.CommandHandlers;
using RapidCMS.ModelMaker.Abstractions.Config;
using RapidCMS.ModelMaker.CommandHandlers.Base;
using RapidCMS.ModelMaker.Models.Commands;
using RapidCMS.ModelMaker.Models.Entities;
using RapidCMS.ModelMaker.Models.Responses;

namespace RapidCMS.ModelMaker.CommandHandlers
{
    internal class RemoveModelEntityCommandHandler : BaseCommandHandler,
            ICommandHandler<RemoveRequest<ModelEntity>, ConfirmResponse>
    {
        public RemoveModelEntityCommandHandler(IModelMakerConfig config) : base(config)
        {
        }

        public Task<ConfirmResponse> HandleAsync(RemoveRequest<ModelEntity> request)
        {
            var fileName = FileName(request.Id);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            return Task.FromResult(new ConfirmResponse
            {
                Success = true
            });
        }
    }
}
