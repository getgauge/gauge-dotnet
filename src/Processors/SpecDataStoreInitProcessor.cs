/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Loaders;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class SpecDataStoreInitProcessor : DataStoreInitProcessorBase, IGaugeProcessor<SpecDataStoreInitRequest, ExecutionStatusResponse>
{
    public SpecDataStoreInitProcessor(IAssemblyLoader loader)
        : base(DataStoreType.Spec, loader)
    {
    }

    public Task<ExecutionStatusResponse> Process(int stream, SpecDataStoreInitRequest request)
    {
        return Task.FromResult(Process(request.Stream));
    }
}