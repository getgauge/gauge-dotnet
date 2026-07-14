/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/



using Gauge.Dotnet.Registries;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class StepNameProcessor : IGaugeProcessor<StepNameRequest, StepNameResponse>
{
    private readonly IStepRegistry _stepRegistry;

    public StepNameProcessor(IStepRegistry stepRegistry)
    {
        _stepRegistry = stepRegistry;
    }

    public Task<StepNameResponse> Process(int stream, StepNameRequest request)
    {

        var parsedStepText = request.StepValue;
        var lookup = _stepRegistry.LookupStep(parsedStepText);
        var response = new StepNameResponse
        {
            IsStepPresent = lookup.Exists
        };

        if (!lookup.Exists) return Task.FromResult(response);

        var info = lookup.Methods[0];
        response.IsExternal = info.IsExternal;
        response.HasAlias = info.HasAlias;
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

        if (info.HasAlias)
            response.StepName.AddRange(info.Aliases);
        else
            response.StepName.Add(info.StepText);

        return Task.FromResult(response);
    }
}