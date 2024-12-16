/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Registries;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class StepPositionsProcessor : IGaugeProcessor<StepPositionsRequest, StepPositionsResponse>
{
    private readonly IStepRegistry _stepRegistry;

    public StepPositionsProcessor(IStepRegistry stepRegistry)
    {
        _stepRegistry = stepRegistry;
    }

    public Task<StepPositionsResponse> Process(int stream, StepPositionsRequest request)
    {
        var response = new StepPositionsResponse();
        response.StepPositions.AddRange(_stepRegistry.GetStepPositions(request.FilePath));
        return Task.FromResult(response);
    }
}