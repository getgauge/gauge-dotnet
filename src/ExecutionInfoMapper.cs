/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Linq;
using Gauge.Messages;

namespace Gauge.Dotnet
{
    public class ExecutionInfoMapper : IExecutionInfoMapper
    {
        private Type _executionContextType;

        public ExecutionInfoMapper(IAssemblyLoader assemblyLoader)
        {
            _executionContextType = assemblyLoader.GetLibType(LibType.ExecutionContext);
        }

        public dynamic ExecutionInfoFrom(ExecutionInfo currentExecutionInfo)
        {
            if (currentExecutionInfo == null)
                return Activator.CreateInstance(_executionContextType);

            return Activator.CreateInstance(_executionContextType, SpecificationFrom(currentExecutionInfo.CurrentSpec),
                ScenarioFrom(currentExecutionInfo.CurrentScenario),
                StepFrom(currentExecutionInfo.CurrentStep));
        }

        private dynamic SpecificationFrom(SpecInfo currentSpec)
        {
            var executionContextSpecType = _executionContextType.GetNestedType("Specification");
            return currentSpec != null
                ? Activator.CreateInstance(executionContextSpecType, currentSpec.Name, currentSpec.FileName, currentSpec.IsFailed,
                    currentSpec.Tags.ToArray())
                : Activator.CreateInstance(executionContextSpecType);
        }

        private dynamic ScenarioFrom(ScenarioInfo currentScenario)
        {
            var executionContextScenarioType = _executionContextType.GetNestedType("Scenario");
            return currentScenario != null
                ? Activator.CreateInstance(executionContextScenarioType, currentScenario.Name, currentScenario.IsFailed,
                    currentScenario.Tags.ToArray())
                : Activator.CreateInstance(executionContextScenarioType);
        }

        private dynamic StepFrom(StepInfo currentStep)
        {
            var executionContextStepType = _executionContextType.GetNestedType("StepDetails"); ;
            if (currentStep == null || currentStep.Step == null)
                return Activator.CreateInstance(executionContextStepType);

            return Activator.CreateInstance(executionContextStepType, currentStep.Step.ActualStepText, currentStep.IsFailed);
        }
    }
}