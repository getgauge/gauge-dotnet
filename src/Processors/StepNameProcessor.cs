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
    public class StepNameProcessor : IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public StepNameProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public Message Process(Message request)
        {
            var parsedStepText = request.StepNameRequest.StepValue;
            var isStepPresent = _stepRegistry.ContainsStep(parsedStepText);
            var message = new Message
            {
                MessageId = request.MessageId,
                MessageType = Message.Types.MessageType.StepNameResponse,
                StepNameResponse = new StepNameResponse
                {
                    IsStepPresent = isStepPresent
                }
            };

            if (!isStepPresent) return message;

            var stepText = _stepRegistry.GetStepText(parsedStepText);
            var hasAlias = _stepRegistry.HasAlias(stepText);
            var info = _stepRegistry.MethodFor(parsedStepText);

            message.StepNameResponse.HasAlias = hasAlias;
            message.StepNameResponse.FileName = info.FileName;
            message.StepNameResponse.Span = new Span
            {
                Start = info.Span.Span.Start.Line + 1,
                StartChar = info.Span.StartLinePosition.Character,
                End = info.Span.EndLinePosition.Line + 1,
                EndChar = info.Span.EndLinePosition.Character
            };

            if (hasAlias)
                message.StepNameResponse.StepName.AddRange(info.Aliases);
            else
                message.StepNameResponse.StepName.Add(stepText);

            return message;
        }
    }
}