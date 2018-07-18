﻿// Copyright 2018 ThoughtWorks, Inc.
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

using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class StepPositionsProcessor : IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public StepPositionsProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public Message Process(Message request)
        {
            var positions = _stepRegistry.GetStepPositions(request.StepPositionsRequest.FilePath);
            var stepPositionsResponse = new StepPositionsResponse();
            stepPositionsResponse.StepPositions.AddRange(positions);
            return new Message {StepPositionsResponse = stepPositionsResponse};
        }
    }
}