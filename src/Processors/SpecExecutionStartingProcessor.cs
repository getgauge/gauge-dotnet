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
    public class SpecExecutionStartingProcessor : UntaggedHooksFirstExecutionProcessor
    {
        private readonly IExecutionOrchestrator _executionOrchestrator;

        public SpecExecutionStartingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
            _executionOrchestrator = executionOrchestrator;
        }

        protected override string HookType => "BeforeSpec";

        protected override List<string> GetApplicableTags(ExecutionInfo info)
        {
            return info.CurrentSpec.Tags.ToList();
        }

        public ExecutionStatusResponse Process(SpecExecutionStartingRequest request)
        {
            _executionOrchestrator.StartExecutionScope("spec");
            return ExecuteHooks(request.CurrentExecutionInfo);
        }
    }
}