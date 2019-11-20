// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-Dotnet.
//
// Gauge-Dotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-Dotnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-Dotnet.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;
using Google.Protobuf;

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

        protected virtual ExecutionStatusResponse ExecuteHooks(ExecutionInfo info)
        {
            var applicableTags = GetApplicableTags(info);
            var mapper = new ExecutionInfoMapper();
            var executionContext = mapper.ExecutionInfoFrom(info);
            var protoExecutionResult =
                ExecutionOrchestrator.ExecuteHooks(HookType, Strategy, applicableTags, executionContext);
            var allPendingMessages = ExecutionOrchestrator.GetAllPendingMessages().Where(m => m != null);
            var allPendingScreenShots = ExecutionOrchestrator.GetAllPendingScreenshots().Select(ByteString.CopyFrom);
            protoExecutionResult.Message.AddRange(allPendingMessages);
            protoExecutionResult.Screenshots.AddRange(allPendingScreenShots);
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