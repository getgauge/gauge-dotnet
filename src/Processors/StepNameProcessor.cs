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


using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class StepNameProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public StepNameProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public StepNameResponse Process(StepNameRequest request)
        {

            var parsedStepText = request.StepValue;
            var isStepPresent = _stepRegistry.ContainsStep(parsedStepText);
            var response = new StepNameResponse
            {
                IsStepPresent = isStepPresent
            };

            if (!isStepPresent) return response;

            var stepText = _stepRegistry.GetStepText(parsedStepText);
            var hasAlias = _stepRegistry.HasAlias(stepText);
            var info = _stepRegistry.MethodFor(parsedStepText);
            response.IsExternal = info.IsExternal;
            response.HasAlias = hasAlias;
            if (!response.IsExternal)
            {
                response.FileName = info.FileName;
                response.Span = new Span
                {
                    Start = info.Span.Span.Start.Line + 1,
                    StartChar = info.Span.StartLinePosition.Character,
                    End = info.Span.EndLinePosition.Line + 1,
                    EndChar = info.Span.EndLinePosition.Character
                };
            }

            if (hasAlias)
                response.StepName.AddRange(info.Aliases);
            else
                response.StepName.Add(stepText);

            return response;
        }
    }
}