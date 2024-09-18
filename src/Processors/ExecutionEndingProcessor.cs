/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Diagnostics;
using Gauge.Dotnet.Executors;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class ExecutionEndingProcessor : HookExecutionProcessor, IGaugeProcessor<ExecutionEndingRequest, ExecutionStatusResponse>
{
    public ExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
    }

    [DebuggerHidden]
    public async Task<ExecutionStatusResponse> Process(int streamId, ExecutionEndingRequest request)
    {
        var result = await ExecuteHooks(streamId, request.CurrentExecutionInfo);
        ClearCacheForConfiguredLevel();
        return result;
    }

    protected override string HookType => "AfterSuite";

    protected override string CacheClearLevel => SuiteLevel;

    protected override List<string> GetApplicableTags(ExecutionInfo info)
    {
        return info?.CurrentSpec?.Tags?.ToList() ?? new List<string>();
    }
}