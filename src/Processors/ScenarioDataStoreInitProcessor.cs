/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.DataStore;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class ScenarioDataStoreInitProcessor(IDataStoreFactory dataStoreFactory) : DataStoreInitProcessorBase(DataStoreType.Scenario, dataStoreFactory),
    IGaugeProcessor<ScenarioDataStoreInitRequest, ExecutionStatusResponse>
{
    public Task<ExecutionStatusResponse> Process(int stream, ScenarioDataStoreInitRequest request)
    {
        return Task.FromResult(Process(request.Stream));
    }
}