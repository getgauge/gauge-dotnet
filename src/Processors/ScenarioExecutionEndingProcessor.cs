/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class ScenarioExecutionEndingProcessor : TaggedHooksFirstExecutionProcessor, IGaugeProcessor<ScenarioExecutionEndingRequest, ExecutionStatusResponse>
{
    private readonly IExecutionOrchestrator _executionOrchestrator;

    public ScenarioExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
        _executionOrchestrator = executionOrchestrator;
    }

    protected override string HookType => "AfterScenario";

    protected override string CacheClearLevel => ScenarioLevel;


    public async Task<ExecutionStatusResponse> Process(int streamId, ScenarioExecutionEndingRequest request)
    {
        _executionOrchestrator.CloseExecutionScope();
        var result = await ExecuteHooks(streamId, request.CurrentExecutionInfo);
        ClearCacheForConfiguredLevel();
        return result;
    }

    protected override List<string> GetApplicableTags(ExecutionInfo info)
    {
        return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
    }

}