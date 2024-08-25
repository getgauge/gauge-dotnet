/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/



using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class StepNameProcessor : IGaugeProcessor<StepNameRequest, StepNameResponse>
{
    private readonly IStepRegistry _stepRegistry;

    public StepNameProcessor(IStepRegistry stepRegistry)
    {
        _stepRegistry = stepRegistry;
    }

    public Task<StepNameResponse> Process(StepNameRequest request)
    {

        var parsedStepText = request.StepValue;
        var isStepPresent = _stepRegistry.ContainsStep(parsedStepText);
        var response = new StepNameResponse
        {
            IsStepPresent = isStepPresent
        };

        if (!isStepPresent) return Task.FromResult(response);

        var stepText = _stepRegistry.GetStepText(parsedStepText);
        var hasAlias = _stepRegistry.HasAlias(stepText);
        var info = _stepRegistry.MethodFor(parsedStepText);
        response.IsExternal = info.IsExternal;
        response.HasAlias = hasAlias;
        if (!response.IsExternal)
        {
            response.FileName = info.FileName;
            response.Span = new Span
            {
                Start = info.Span.Span.Start.Line + 1,
                StartChar = info.Span.StartLinePosition.Character,
                End = info.Span.EndLinePosition.Line + 1,
                EndChar = info.Span.EndLinePosition.Character
            };
        }

        if (hasAlias)
            response.StepName.AddRange(info.Aliases);
        else
            response.StepName.Add(stepText);

        return Task.FromResult(response);
    }
}