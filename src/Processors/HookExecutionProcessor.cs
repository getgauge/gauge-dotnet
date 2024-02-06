/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public abstract class HookExecutionProcessor : ExecutionProcessor
    {
        private const string ClearStateFlag = "gauge_clear_state_level";
        protected const string SuiteLevel = "suite";
        protected const string SpecLevel = "spec";
        protected const string ScenarioLevel = "scenario";
        protected readonly IExecutionOrchestrator ExecutionOrchestrator;

        protected HookExecutionProcessor(IExecutionOrchestrator executionOrchestrator)
        {
            ExecutionOrchestrator = executionOrchestrator;
            Strategy = new HooksStrategy();
        }

        protected HooksStrategy Strategy { get; set; }

        protected abstract string HookType { get; }

        protected virtual string CacheClearLevel => null;

        protected virtual async Task<ExecutionStatusResponse> ExecuteHooks(ExecutionInfo info)
        {
            var applicableTags = GetApplicableTags(info);
            var protoExecutionResult =
                await ExecutionOrchestrator.ExecuteHooks(HookType, Strategy, applicableTags, info);
            var allPendingMessages = ExecutionOrchestrator.GetAllPendingMessages().Where(m => m != null);
            var allPendingScreenShotFiles = ExecutionOrchestrator.GetAllPendingScreenshotFiles();
            protoExecutionResult.Message.AddRange(allPendingMessages);
            protoExecutionResult.ScreenshotFiles.AddRange(allPendingScreenShotFiles);
            return new ExecutionStatusResponse { ExecutionResult = protoExecutionResult };
        }

        protected void ClearCacheForConfiguredLevel()
        {
            var flag = Utils.TryReadEnvValue(ClearStateFlag);
            if (!string.IsNullOrEmpty(flag) && flag.Trim().Equals(CacheClearLevel))
                ExecutionOrchestrator.ClearCache();
        }

        protected virtual List<string> GetApplicableTags(ExecutionInfo info)
        {
            return Enumerable.Empty<string>().ToList();
        }
    }
}