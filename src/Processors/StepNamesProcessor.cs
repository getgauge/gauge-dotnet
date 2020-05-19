/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class StepNamesProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public StepNamesProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public StepNamesResponse Process(StepNamesRequest request)
        {
            var allSteps = _stepRegistry.GetStepTexts();
            var stepNamesResponse = new StepNamesResponse();
            stepNamesResponse.Steps.AddRange(allSteps);
            return stepNamesResponse;
        }
    }
}