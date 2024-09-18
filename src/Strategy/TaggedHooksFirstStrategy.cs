/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;

namespace Gauge.Dotnet.Strategy;

[Serializable]
public class TaggedHooksFirstStrategy : HooksStrategy
{
    public override IEnumerable<string> GetApplicableHooks(IEnumerable<string> applicableTags, IEnumerable<IHookMethod> hooks)
    {
        var hookMethods = hooks as IList<IHookMethod> ?? hooks.ToList();
        var tags = applicableTags as IList<string> ?? applicableTags.ToList();
        return tags.Any()
            ? GetTaggedHooks(tags, hookMethods).Concat(GetUntaggedHooks(hookMethods))
            : GetUntaggedHooks(hookMethods);
    }
}