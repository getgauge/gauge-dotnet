/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class StepNamesProcessor : IGaugeProcessor<StepNamesRequest, StepNamesResponse>
{
    private readonly IStepRegistry _stepRegistry;

    public StepNamesProcessor(IStepRegistry stepRegistry)
    {
        _stepRegistry = stepRegistry;
    }

    public Task<StepNamesResponse> Process(int stream, StepNamesRequest request)
    {
        var allSteps = _stepRegistry.GetStepTexts();
        var stepNamesResponse = new StepNamesResponse();
        stepNamesResponse.Steps.AddRange(allSteps);
        return Task.FromResult(stepNamesResponse);
    }
}