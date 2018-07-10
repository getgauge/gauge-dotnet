// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class StepValidationProcessor : IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public StepValidationProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public Message Process(Message request)
        {
            var stepToValidate = request.StepValidateRequest.StepText;
            var isValid = true;
            var errorMessage = "";
            string suggestion = "";
            var errorType = StepValidateResponse.Types.ErrorType.StepImplementationNotFound;
            if (!_stepRegistry.ContainsStep(stepToValidate))
            {
                isValid = false;
                errorMessage = string.Format("No implementation found for : {0}. Full Step Text :", stepToValidate);
                suggestion = GetSuggestion(request.StepValidateRequest.StepValue);
            }
            else if (_stepRegistry.HasMultipleImplementations(stepToValidate))
            {
                isValid = false;
                errorType = StepValidateResponse.Types.ErrorType.DuplicateStepImplementation;
                errorMessage = string.Format("Multiple step implementations found for : {0}", stepToValidate);
            }

            return GetStepValidateResponseMessage(isValid, request, errorType, errorMessage, suggestion);
        }

        private string GetSuggestion(ProtoStepValue stepValue)
        {
            var name = stepValue.StepValue.ToValidCSharpIdentifier();
            return "\t\t[Step(\"" + stepValue.ParameterizedStepValue + "\")]\n" +
                   "\t\tpublic void " + name + "(" + GetParamsList(stepValue.Parameters) + ")\n" +
                   "\t\t{\n\t\t\tthrow new NotImplementedException();\n\t\t}\n";
        }

        private static string GetParamsList(IEnumerable<string> stepValueParameters)
        {
            var paramsString = stepValueParameters.Select((p, i) => $"arg{i}");
            return string.Join(" ,", paramsString);
        }

        private static Message GetStepValidateResponseMessage(bool isValid, Message request,
            StepValidateResponse.Types.ErrorType errorType, string errorMessage, string suggestion)
        {
            var stepValidateResponse = new StepValidateResponse
            {
                ErrorMessage = errorMessage,
                IsValid = isValid,
                ErrorType = errorType,
                Suggestion = suggestion
            };
            return new Message
            {
                MessageId = request.MessageId,
                MessageType = Message.Types.MessageType.StepValidateResponse,
                StepValidateResponse = stepValidateResponse
            };
        }
    }
}