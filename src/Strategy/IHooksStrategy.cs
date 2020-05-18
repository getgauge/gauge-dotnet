/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using Gauge.Dotnet.Models;

namespace Gauge.Dotnet.Strategy
{
    public interface IHooksStrategy
    {
        IEnumerable<string> GetTaggedHooks(IEnumerable<string> applicableTags, IList<IHookMethod> hooks);
        IEnumerable<string> GetApplicableHooks(IEnumerable<string> applicableTags, IEnumerable<IHookMethod> hooks);
    }
}