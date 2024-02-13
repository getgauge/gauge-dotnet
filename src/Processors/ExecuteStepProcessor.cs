/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Diagnostics;
using System.Linq;
using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class ExecuteStepProcessor
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
        public ExecutionStatusResponse Process(ExecuteStepRequest request)
        {
            if (!_stepRegistry.ContainsStep(request.ParsedStepText))
                return ExecutionError("Step Implementation not found");

            var method = _stepRegistry.MethodFor(request.ParsedStepText);

            var parameters = method.ParameterCount;
            var args = new string[parameters];
            var stepParameter = request.Parameters;
            if (parameters != stepParameter.Count)
            {
                var argumentMismatchError = string.Format(
                    "Argument length mismatch for {0}. Actual Count: {1}, Expected Count: {2}",
                    request.ActualStepText,
                    stepParameter.Count, parameters);
                return ExecutionError(argumentMismatchError);
            }

            var validTableParamTypes = new[]
                {Parameter.Types.ParameterType.Table, Parameter.Types.ParameterType.SpecialTable};

            for (var i = 0; i < parameters; i++)
                args[i] = validTableParamTypes.Contains(stepParameter[i].ParameterType)
                    ? _tableFormatter.GetJSON(stepParameter[i].Table)
                    : stepParameter[i].Value;
            var protoExecutionResult = _executionOrchestrator.ExecuteStep(method, args);
            return new ExecutionStatusResponse { ExecutionResult = protoExecutionResult };
        }

        private static ExecutionStatusResponse ExecutionError(string errorMessage)
        {
            var result = new ProtoExecutionResult
            {
                Failed = true,
                RecoverableError = false,
                ExecutionTime = 0,
                ErrorMessage = errorMessage
            };
            return new ExecutionStatusResponse { ExecutionResult = result };
        }
    }
}