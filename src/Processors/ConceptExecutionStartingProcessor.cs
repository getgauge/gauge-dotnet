﻿/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class ConceptExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor
    {
        public ConceptExecutionStartingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
        }

        protected override string HookType => "BeforeConcept";

        public ExecutionStatusResponse Process(ConceptExecutionStartingRequest request)
        {
            return ExecuteHooks(request.CurrentExecutionInfo);
        }

        protected override List<string> GetApplicableTags(ExecutionInfo info)
        {
            return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
        }
    }
}