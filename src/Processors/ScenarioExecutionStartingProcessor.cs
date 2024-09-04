/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class ScenarioExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor, IGaugeProcessor<ScenarioExecutionStartingRequest, ExecutionStatusResponse>
{
    private readonly IExecutionOrchestrator _executionOrchestrator;

    public ScenarioExecutionStartingProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
        _executionOrchestrator = executionOrchestrator;
    }

    protected override string HookType => "BeforeScenario";

    public async Task<ExecutionStatusResponse> Process(int streamId, ScenarioExecutionStartingRequest request)
    {
        _executionOrchestrator.StartExecutionScope("scenario");
        return await ExecuteHooks(streamId, request.CurrentExecutionInfo);
    }

    protected override List<string> GetApplicableTags(ExecutionInfo info)
    {
        return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
    }
}