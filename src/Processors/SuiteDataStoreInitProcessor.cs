/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Loaders;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class SuiteDataStoreInitProcessor : DataStoreInitProcessorBase, IGaugeProcessor<SuiteDataStoreInitRequest, ExecutionStatusResponse>
{
    public SuiteDataStoreInitProcessor(IAssemblyLoader loader)
        : base(DataStoreType.Suite, loader)
    {
    }

    public Task<ExecutionStatusResponse> Process(int stream, SuiteDataStoreInitRequest request)
    {
        return Task.FromResult(Process(request.Stream));
    }
}