/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class ConceptExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor, IGaugeProcessor<ConceptExecutionStartingRequest, ExecutionStatusResponse>
{
    public ConceptExecutionStartingProcessor(IExecutionOrchestrator executionOrchestrator, IConfiguration config)
        : base(executionOrchestrator, config)
    {
    }

    protected override string HookType => "BeforeConcept";

    public async Task<ExecutionStatusResponse> Process(int streamId, ConceptExecutionStartingRequest request)
    {
        return await ExecuteHooks(streamId, request.CurrentExecutionInfo);
    }

    protected override List<string> GetApplicableTags(ExecutionInfo info)
    {
        return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
    }
}