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

using System;
using System.Linq;
using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class RefactorProcessor : IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public RefactorProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public Message Process(Message request)
        {
            var newStep = request.RefactorRequest.NewStepValue;

            var newStepValue = newStep.ParameterizedStepValue;
            var parameterPositions = request.RefactorRequest.ParamPositions
                .Select(position => new Tuple<int, int>(position.OldPosition, position.NewPosition)).ToList();

            var response = new RefactorResponse();
            try
            {
                var gaugeMethod = GetGaugeMethod(request.RefactorRequest.OldStepValue);
                var changedFile = RefactorHelper.Refactor(gaugeMethod, parameterPositions, newStep.Parameters.ToList(),
                    newStepValue);
                response.Success = true;
                response.FilesChanged.Add(changedFile);
            }
            catch (AggregateException ex)
            {
                response.Success = false;
                response.Error = ex.InnerExceptions.Select(exception => exception.Message).Distinct()
                    .Aggregate((s, s1) => string.Concat(s, "; ", s1));
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
            }


            return new Message
            {
                MessageId = request.MessageId,
                MessageType = Message.Types.MessageType.RefactorResponse,
                RefactorResponse = response
            };
        }

        private GaugeMethod GetGaugeMethod(ProtoStepValue stepValue)
        {
            if (_stepRegistry.HasMultipleImplementations(stepValue.StepValue))
                throw new Exception(string.Format("Multiple step implementations found for : {0}",
                    stepValue.ParameterizedStepValue));
            return _stepRegistry.MethodFor(stepValue.StepValue);
        }
    }
}