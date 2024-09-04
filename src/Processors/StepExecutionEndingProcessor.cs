/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class StepExecutionEndingProcessor : TaggedHooksFirstExecutionProcessor, IGaugeProcessor<StepExecutionEndingRequest, ExecutionStatusResponse>
{
    public StepExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
    }

    protected override string HookType => "AfterStep";

    public Task<ExecutionStatusResponse> Process(int streamId, StepExecutionEndingRequest request)
    {
        return ExecuteHooks(streamId, request.CurrentExecutionInfo);
    }

    protected override List<string> GetApplicableTags(ExecutionInfo info)
    {
        return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
    }
}