/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using Gauge.Messages;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;

namespace Gauge.Dotnet
{
    public interface IHookExecutor
    {
        ExecutionResult Execute(string hookType, IHooksStrategy strategy, IList<string> applicableTags,
            ExecutionInfo context);
    }
}