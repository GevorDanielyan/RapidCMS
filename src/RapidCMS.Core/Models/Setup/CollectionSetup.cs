﻿using System;
using System.Collections.Generic;
using System.Linq;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Extensions;

namespace RapidCMS.Core.Models.Setup
{
    public class CollectionSetup
    {
        public CollectionSetup(
            string? icon,
            string? color,
            string name,
            string alias,
            string repositoryAlias)
        {
            Icon = icon;
            Color = color;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
            RepositoryAlias = repositoryAlias ?? throw new ArgumentNullException(nameof(repositoryAlias));
            Validators = new List<ValidationSetup>();
        }

        public string? Icon { get; private set; }
        public string? Color { get; private set; }
        public string Name { get; private set; }
        public string Alias { get; private set; }
        public string RepositoryAlias { get; private set; }

        public UsageType UsageType { get; set; }

        public TreeElementSetup? Parent { get; set; }
        public List<TreeElementSetup> Collections { get; set; } = new List<TreeElementSetup>();

        public List<EntityVariantSetup>? SubEntityVariants { get; set; }
        public EntityVariantSetup EntityVariant { get; set; } = EntityVariantSetup.Undefined;

        public List<IDataView>? DataViews { get; set; }
        public Type? DataViewBuilder { get; set; }

        public EntityVariantSetup GetEntityVariant(string? alias)
        {
            if (string.IsNullOrWhiteSpace(alias) || SubEntityVariants == null || EntityVariant.Alias == alias)
            {
                return EntityVariant;
            }
            else
            {
                return SubEntityVariants.FirstOrDefault(x => x.Alias == alias) ?? throw new InvalidOperationException($"Entity variant with alias {alias} does not exist.");
            }
        }
        public EntityVariantSetup GetEntityVariant(IEntity entity)
        {
            return SubEntityVariants?.FirstOrDefault(x => x.Type == entity.GetType())
                ?? EntityVariant;
        }

        public TreeViewSetup? TreeView { get; set; }
        public ElementSetup? ElementSetup { get; set; }

        public ListSetup? ListView { get; set; }
        public ListSetup? ListEditor { get; set; }

        public NodeSetup? NodeView { get; set; }
        public NodeSetup? NodeEditor { get; set; }

        public List<ValidationSetup> Validators { get; set; }

        public ButtonSetup? FindButton(string buttonId)
            => EnumerableExtensions
                .MergeAll(
                    ListView?.GetAllButtons(),
                    ListEditor?.GetAllButtons(),
                    NodeView?.GetAllButtons(),
                    NodeEditor?.GetAllButtons())
                .FirstOrDefault(x => x.ButtonId == buttonId);
    }
}
