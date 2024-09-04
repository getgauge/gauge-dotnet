/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;

namespace Gauge.Dotnet.Executors;

public interface IStepExecutor
{
    Task<ExecutionResult> Execute(GaugeMethod gaugeMethod, int streamId, string[] args);
}