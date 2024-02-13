/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;

namespace Gauge.Dotnet
{
    public interface IStepExecutor
    {
        ExecutionResult Execute(GaugeMethod gaugeMethod, string[] args);
    }
}