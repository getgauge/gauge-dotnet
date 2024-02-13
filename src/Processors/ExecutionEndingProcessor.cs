/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class ExecutionEndingProcessor : HookExecutionProcessor
    {
        public ExecutionEndingProcessor(IExecutionOrchestrator executionOrchestrator)
            : base(executionOrchestrator)
        {
        }

        [DebuggerHidden]
        public virtual ExecutionStatusResponse Process(ExecutionEndingRequest request)
        {
            var result = ExecuteHooks(request.CurrentExecutionInfo);
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
}