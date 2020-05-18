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
    public class ScenarioExecutionEndingProcessor : TaggedHooksFirstExecutionProcessor
    {
        private readonly IExecutionOrchestrator _executionOrchestrator;

        public ScenarioExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
            _executionOrchestrator = executionOrchestrator;
        }

        protected override string HookType => "AfterScenario";

        protected override string CacheClearLevel => ScenarioLevel;


        public ExecutionStatusResponse Process(ScenarioExecutionEndingRequest request)
        {
            _executionOrchestrator.CloseExecutionScope();
            return ExecuteHooks(request.CurrentExecutionInfo);
        }

        protected override List<string> GetApplicableTags(ExecutionInfo info)
        {
            return info.CurrentScenario.Tags.Union(info.CurrentSpec.Tags).ToList();
        }

    }
}