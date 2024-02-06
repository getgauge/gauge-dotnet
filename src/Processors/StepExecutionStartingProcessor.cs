/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class StepExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor
    {
        public StepExecutionStartingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
        }

        protected override string HookType => "BeforeStep";

        public async Task<ExecutionStatusResponse> Process(StepExecutionStartingRequest request)
        {
            return await ExecuteHooks(request.CurrentExecutionInfo);
        }

        protected override List<string> GetApplicableTags(ExecutionInfo info)
        {
            return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
        }
    }
}