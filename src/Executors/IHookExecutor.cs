/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;

namespace Gauge.Dotnet.Executors;

public interface IHookExecutor
{
    Task<ExecutionResult> Execute(string hookType, IHooksStrategy strategy, IList<string> applicableTags, int streamId, ExecutionInfo context);
}