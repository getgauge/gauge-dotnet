/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class StepExecutionEndingProcessor : TaggedHooksFirstExecutionProcessor
    {
        public StepExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
        }

        protected override string HookType => "AfterStep";

        public ExecutionStatusResponse Process(StepExecutionEndingRequest request)
        {
            return base.ExecuteHooks(request.CurrentExecutionInfo);
        }

        protected override List<string> GetApplicableTags(ExecutionInfo info)
        {
            return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
        }
    }

    public class ConceptExecutionEndingProcessor : TaggedHooksFirstExecutionProcessor
    {
        public ConceptExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
        }

        protected override string HookType => "AfterStep";

        public Empty Process(ConceptExecutionEndingRequest request)
        {
            base.ExecuteHooks(request.CurrentExecutionInfo);
            return new Empty();
        }

        protected override List<string> GetApplicableTags(ExecutionInfo info)
        {
            return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
        }
    }
}