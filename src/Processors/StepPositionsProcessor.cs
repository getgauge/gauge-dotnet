/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class StepPositionsProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public StepPositionsProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }
        public StepPositionsResponse Process(StepPositionsRequest request)
        {
            var response = new StepPositionsResponse();
            response.StepPositions.AddRange(_stepRegistry.GetStepPositions(request.FilePath));
            return response;
        }
    }
}