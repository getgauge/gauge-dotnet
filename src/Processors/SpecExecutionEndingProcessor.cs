/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class SpecExecutionEndingProcessor : TaggedHooksFirstExecutionProcessor, IGaugeProcessor<SpecExecutionEndingRequest, ExecutionStatusResponse>
{
    private readonly IExecutionOrchestrator _executionOrchestrator;

    public SpecExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
        _executionOrchestrator = executionOrchestrator;
    }

    protected override string HookType => "AfterSpec";

    protected override string CacheClearLevel => SpecLevel;
    protected override List<string> GetApplicableTags(ExecutionInfo info)
    {
        return info.CurrentSpec.Tags.ToList();
    }

    public async Task<ExecutionStatusResponse> Process(int streamId, SpecExecutionEndingRequest request)
    {
        _executionOrchestrator.CloseExecutionScope();
        var result = await ExecuteHooks(streamId, request.CurrentExecutionInfo);
        ClearCacheForConfiguredLevel();
        return result;
    }
}