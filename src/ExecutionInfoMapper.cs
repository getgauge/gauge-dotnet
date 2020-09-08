/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Linq;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet
{
    public class ExecutionInfoMapper : IExecutionInfoMapper
    {
        private Type _executionContextType;
        private readonly IActivatorWrapper activatorWrapper;

        public ExecutionInfoMapper(IAssemblyLoader assemblyLoader, IActivatorWrapper activatorWrapper)
        {
            _executionContextType = assemblyLoader.GetLibType(LibType.ExecutionContext);
            this.activatorWrapper = activatorWrapper;
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
            return currentScenario != null
                ? activatorWrapper.CreateInstance(executionContextScenarioType, currentScenario.Name, currentScenario.IsFailed,
                    currentScenario.Tags.ToArray())
                : activatorWrapper.CreateInstance(executionContextScenarioType);
        }

        private dynamic StepFrom(StepInfo currentStep)
        {
            var executionContextStepType = _executionContextType.GetNestedType("StepDetails"); ;
            if (currentStep == null || currentStep.Step == null)
                return activatorWrapper.CreateInstance(executionContextStepType);

            return activatorWrapper.CreateInstance(executionContextStepType, currentStep.Step.ActualStepText, currentStep.IsFailed);
        }
    }
}