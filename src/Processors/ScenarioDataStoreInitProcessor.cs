/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Loaders;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class ScenarioDataStoreInitProcessor : DataStoreInitProcessorBase, IGaugeProcessor<ScenarioDataStoreInitRequest, ExecutionStatusResponse>
{
    public ScenarioDataStoreInitProcessor(IAssemblyLoader loader)
        : base(DataStoreType.Scenario, loader)
    {
    }

    public Task<ExecutionStatusResponse> Process(int stream, ScenarioDataStoreInitRequest request)
    {
        return Task.FromResult(Process(request.Stream));
    }
}