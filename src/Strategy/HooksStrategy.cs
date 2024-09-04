/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;

namespace Gauge.Dotnet.Strategy;

[Serializable]
public class HooksStrategy : IHooksStrategy
{
    public IEnumerable<string> GetTaggedHooks(IEnumerable<string> applicableTags, IList<IHookMethod> hooks)
    {
        var tagsList = applicableTags.ToList();
        return from hookMethod in hooks.ToList()
               where hookMethod.FilterTags != null && hookMethod.FilterTags.Any()
               where
                   // TagAggregation.And=0, Or=1
                   hookMethod.TagAggregation == 1 && hookMethod.FilterTags.Intersect(tagsList).Any() ||
                   hookMethod.TagAggregation == 0 && hookMethod.FilterTags.All(tagsList.Contains)
               orderby hookMethod.Method
               select hookMethod.Method;
    }

    public virtual IEnumerable<string> GetApplicableHooks(IEnumerable<string> applicableTags, IEnumerable<IHookMethod> hooks)
    {
        return GetUntaggedHooks(hooks);
    }

    protected IOrderedEnumerable<string> GetUntaggedHooks(IEnumerable<IHookMethod> hookMethods)
    {
        return hookMethods.Where(method => method.FilterTags == null || !method.FilterTags.Any())
            .Select(method => method.Method)
            .OrderBy(info => info);
    }
}