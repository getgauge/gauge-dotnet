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

using System.Diagnostics;
using System.Linq;
using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class ExecuteStepProcessor : ExecutionProcessor, IMessageProcessor
    {
        private readonly IExecutionOrchestrator _executionOrchestrator;
        private readonly IStepRegistry _stepRegistry;
        private readonly ITableFormatter _tableFormatter;

        public ExecuteStepProcessor(IStepRegistry registry, IExecutionOrchestrator executionOrchestrator,
            ITableFormatter tableFormatter)
        {
            _stepRegistry = registry;
            _tableFormatter = tableFormatter;
            _executionOrchestrator = executionOrchestrator;
        }

        [DebuggerHidden]
        public Message Process(Message request)
        {
            var executeStepRequest = request.ExecuteStepRequest;
            if (!_stepRegistry.ContainsStep(executeStepRequest.ParsedStepText))
                return ExecutionError("Step Implementation not found", request);

            var method = _stepRegistry.MethodFor(executeStepRequest.ParsedStepText);

            var parameters = method.ParameterCount;
            var args = new string[parameters];
            var stepParameter = executeStepRequest.Parameters;
            if (parameters != stepParameter.Count)
            {
                var argumentMismatchError = string.Format(
                    "Argument length mismatch for {0}. Actual Count: {1}, Expected Count: {2}",
                    executeStepRequest.ActualStepText,
                    stepParameter.Count, parameters);
                return ExecutionError(argumentMismatchError, request);
            }

            var validTableParamTypes = new[]
                {Parameter.Types.ParameterType.Table, Parameter.Types.ParameterType.SpecialTable};

            for (var i = 0; i < parameters; i++)
                args[i] = validTableParamTypes.Contains(stepParameter[i].ParameterType)
                    ? _tableFormatter.GetJSON(stepParameter[i].Table)
                    : stepParameter[i].Value;
            var protoExecutionResult = _executionOrchestrator.ExecuteStep(method, args);
            return WrapInMessage(protoExecutionResult, request);
        }

        private static Message ExecutionError(string errorMessage, Message request)
        {
            var result = new ProtoExecutionResult
            {
                Failed = true,
                RecoverableError = false,
                ExecutionTime = 0,
                ErrorMessage = errorMessage
            };
            return WrapInMessage(result, request);
        }
    }
}