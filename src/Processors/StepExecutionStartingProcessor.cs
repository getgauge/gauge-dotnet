/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class StepExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor, IGaugeProcessor<StepExecutionStartingRequest, ExecutionStatusResponse>
{
    public StepExecutionStartingProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
    }

    protected override string HookType => "BeforeStep";

    public async Task<ExecutionStatusResponse> Process(int streamId, StepExecutionStartingRequest request)
    {
        return await ExecuteHooks(streamId, request.CurrentExecutionInfo);
    }

    protected override List<string> GetApplicableTags(ExecutionInfo info)
    {
        return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
    }
}