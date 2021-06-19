﻿using System.ComponentModel.DataAnnotations;
using RapidCMS.Core.Extensions;
using RapidCMS.ModelMaker.Core.Abstractions.Validation;
using RapidCMS.ModelMaker.Models.Entities;

namespace RapidCMS.ModelMaker.Validation.Config
{
    public class MinLengthValidationConfig : IValidatorConfig
    {
        [Range(1, int.MaxValue)]
        public int? MinLength { get; set; }

        public bool IsEnabled => MinLength.HasValue;
        public bool AlwaysIncluded => false;

        public bool IsApplicable(PropertyModel model)
            => model.EditorAlias.In(Constants.Editors.TextArea, Constants.Editors.TextBox);

        public string? RelatedCollectionAlias => default;

        public string? ValidationAttributeExpression => $"[MinLength({MinLength})]";

        public string? DataCollectionExpression => default;
    }
}