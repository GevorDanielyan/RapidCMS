﻿using System.Collections.Generic;
using System.Threading.Tasks;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Models.Setup;

namespace RapidCMS.Core.Extensions
{
    internal static class SetupResolverExtension
    {
        public static async Task<IResolvedSetup<IEnumerable<TSetup>>> ResolveSetupAsync<TSetup, TConfig>(this ISetupResolver<TSetup, TConfig> resolver, IEnumerable<TConfig> configs, CollectionSetup? collection = default)
            where TConfig : notnull
        {
            var allCachable = true;
            return new ResolvedSetup<IEnumerable<TSetup>>(
                await configs.ToListAsync(async config => (await resolver.ResolveSetupAsync(config, collection)).CheckIfCachable(ref allCachable)), allCachable);
        }

        public static TSetup CheckIfCachable<TSetup>(this IResolvedSetup<TSetup> resolvedSetup, ref bool cachable)
        {
            cachable = cachable && resolvedSetup.Cachable;

            return resolvedSetup.Setup;
        }
    }
}
