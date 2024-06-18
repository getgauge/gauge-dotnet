/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Linq;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Processors;
using System.Diagnostics.Tracing;
using Microsoft.VisualBasic;

namespace Gauge.Dotnet
{
    public class ExecutionInfoMapper : IExecutionInfoMapper
    {
        private Type _executionContextType;
        private readonly IActivatorWrapper activatorWrapper;
        private readonly ITableFormatter tableFormatter;

        public ExecutionInfoMapper(IAssemblyLoader assemblyLoader, IActivatorWrapper activatorWrapper)
        {
            _executionContextType = assemblyLoader.GetLibType(LibType.ExecutionContext);
            this.activatorWrapper = activatorWrapper;
            tableFormatter = new TableFormatter(assemblyLoader, activatorWrapper);
        }

        public dynamic ExecutionContextFrom(ExecutionInfo currentExecutionInfo)
        {
            if (currentExecutionInfo == null)
                return activatorWrapper.CreateInstance(_executionContextType);

            return activatorWrapper.CreateInstance(_executionContextType, SpecificationFrom(currentExecutionInfo.CurrentSpec),
                ScenarioFrom(currentExecutionInfo.CurrentScenario),
                StepFrom(currentExecutionInfo.CurrentStep));
        }

        private dynamic SpecificationFrom(SpecInfo currentSpec)
        {
            var executionContextSpecType = _executionContextType.GetNestedType("Specification");
            return currentSpec != null
                ? activatorWrapper.CreateInstance(executionContextSpecType, currentSpec.Name, currentSpec.FileName, currentSpec.IsFailed,
                    currentSpec.Tags.ToArray())
                : activatorWrapper.CreateInstance(executionContextSpecType);
        }

        private dynamic ScenarioFrom(ScenarioInfo currentScenario)
        {
            var executionContextScenarioType = _executionContextType.GetNestedType("Scenario");
            if (currentScenario != null && currentScenario.Retries == null) 
            {
                currentScenario.Retries = new ScenarioRetriesInfo{MaxRetries=0, CurrentRetry=0};
            }
            return currentScenario != null
                ? activatorWrapper.CreateInstance(executionContextScenarioType, currentScenario.Name, currentScenario.IsFailed,
                    currentScenario.Tags.ToArray(), currentScenario.Retries.MaxRetries, currentScenario.Retries.CurrentRetry)
                : activatorWrapper.CreateInstance(executionContextScenarioType);
        }

        private dynamic StepFrom(StepInfo currentStep)
        {
            var executionContextStepType = _executionContextType.GetNestedType("StepDetails"); ;
            if (currentStep == null || currentStep.Step == null)
                return activatorWrapper.CreateInstance(executionContextStepType);

            var parameters = new List<List<string>>();
            foreach (var parameter in currentStep.Step.Parameters) {
                if (parameter.ParameterType == Parameter.Types.ParameterType.Static) {
                    parameters.Add(new List<string> { "Static", parameter.Name, parameter.Value });
                }
                else if (parameter.ParameterType == Parameter.Types.ParameterType.Dynamic) {
                    parameters.Add(new List<string> { "Dynamic", parameter.Name, parameter.Value });
                }
                else if (parameter.ParameterType == Parameter.Types.ParameterType.SpecialString) {
                    parameters.Add(new List<string> { "Special", parameter.Name, parameter.Value });
                }
                else if (parameter.ParameterType == Parameter.Types.ParameterType.SpecialTable ||
                    parameter.ParameterType == Parameter.Types.ParameterType.Table) {
                    var asJSon = tableFormatter.GetJSON(parameter.Table);
                    parameters.Add(new List<string> { "Table", parameter.Name, asJSon });
                }
            }

            var inst = activatorWrapper.CreateInstance(
                executionContextStepType, 
                currentStep.Step.ActualStepText, currentStep.IsFailed, 
                currentStep.StackTrace, currentStep.ErrorMessage, 
                parameters);
            
            return inst;
        }
    }
}