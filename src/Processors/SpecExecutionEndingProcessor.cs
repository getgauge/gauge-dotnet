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
    public class SpecExecutionEndingProcessor : TaggedHooksFirstExecutionProcessor
    {
        private readonly IExecutionOrchestrator _executionOrchestrator;

        public SpecExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
            _executionOrchestrator = executionOrchestrator;
        }

        protected override string HookType => "AfterSpec";

        protected override string CacheClearLevel => SpecLevel;
        protected override List<string> GetApplicableTags(ExecutionInfo info)
        {
            return info.CurrentSpec.Tags.ToList();
        }

        public async Task<ExecutionStatusResponse> Process(SpecExecutionEndingRequest request)
        {
            _executionOrchestrator.CloseExecutionScope();
            var result = await ExecuteHooks(request.CurrentExecutionInfo);
            ClearCacheForConfiguredLevel();
            return result;
        }
    }
}