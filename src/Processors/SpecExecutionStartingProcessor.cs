/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Executors;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class SpecExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor, IGaugeProcessor<SpecExecutionStartingRequest, ExecutionStatusResponse>
{
    private readonly IExecutionOrchestrator _executionOrchestrator;

    public SpecExecutionStartingProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
        _executionOrchestrator = executionOrchestrator;
    }

    protected override string HookType => "BeforeSpec";

    protected override List<string> GetApplicableTags(ExecutionInfo info)
    {
        return info.CurrentSpec.Tags.ToList();
    }

    public async Task<ExecutionStatusResponse> Process(int streamId, SpecExecutionStartingRequest request)
    {
        _executionOrchestrator.StartExecutionScope("spec");
        return await ExecuteHooks(streamId, request.CurrentExecutionInfo);
    }
}