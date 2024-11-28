/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.DataStore;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class SuiteDataStoreInitProcessor(IDataStoreFactory dataStoreFactory) : DataStoreInitProcessorBase(DataStoreType.Suite, dataStoreFactory),
    IGaugeProcessor<SuiteDataStoreInitRequest, ExecutionStatusResponse>
{
    public Task<ExecutionStatusResponse> Process(int stream, SuiteDataStoreInitRequest request)
    {
        return Task.FromResult(Process(request.Stream));
    }
}