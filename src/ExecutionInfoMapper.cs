/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Linq;
using Gauge.CSharp.Lib;
using Gauge.Messages;

namespace Gauge.Dotnet
{
    public class ExecutionInfoMapper
    {
        public ExecutionContext ExecutionInfoFrom(ExecutionInfo currentExecutionInfo)
        {
            if (currentExecutionInfo == null)
                return new ExecutionContext();

            return new ExecutionContext(SpecificationFrom(currentExecutionInfo.CurrentSpec),
                ScenarioFrom(currentExecutionInfo.CurrentScenario),
                StepFrom(currentExecutionInfo.CurrentStep));
        }

        public ExecutionContext.Specification SpecificationFrom(SpecInfo currentSpec)
        {
            return currentSpec != null
                ? new ExecutionContext.Specification(currentSpec.Name, currentSpec.FileName, currentSpec.IsFailed,
                    currentSpec.Tags.ToArray())
                : new ExecutionContext.Specification();
        }

        public ExecutionContext.Scenario ScenarioFrom(ScenarioInfo currentScenario)
        {
            return currentScenario != null
                ? new ExecutionContext.Scenario(currentScenario.Name, currentScenario.IsFailed,
                    currentScenario.Tags.ToArray())
                : new ExecutionContext.Scenario();
        }

        public ExecutionContext.StepDetails StepFrom(StepInfo currentStep)
        {
            if (currentStep == null || currentStep.Step == null)
                return new ExecutionContext.StepDetails();

            return new ExecutionContext.StepDetails(currentStep.Step.ActualStepText, currentStep.IsFailed);
        }
    }
}