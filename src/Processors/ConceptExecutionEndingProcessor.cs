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
    public class ConceptExecutionEndingProcessor : TaggedHooksFirstExecutionProcessor
    {
        public ConceptExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
        }

        protected override string HookType => "AfterConcept";

        public ExecutionStatusResponse Process(ConceptExecutionEndingRequest request)
        {
            return base.ExecuteHooks(request.CurrentExecutionInfo);
        }

        protected override List<string> GetApplicableTags(ExecutionInfo info)
        {
            return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
        }
    }
}