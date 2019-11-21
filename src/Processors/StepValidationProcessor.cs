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

using System.Collections.Generic;
using System.Linq;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class StepValidationProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public StepValidationProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public StepValidateResponse Process(StepValidateRequest request)
        {
            var stepToValidate = request.StepText;
            var isValid = true;
            var errorMessage = "";
            var suggestion = "";
            var errorType = StepValidateResponse.Types.ErrorType.StepImplementationNotFound;
            if (!_stepRegistry.ContainsStep(stepToValidate))
            {
                isValid = false;
                errorMessage = string.Format("No implementation found for : {0}. Full Step Text :", stepToValidate);
                suggestion = GetSuggestion(request.StepValue);
            }
            else if (_stepRegistry.HasMultipleImplementations(stepToValidate))
            {
                isValid = false;
                errorType = StepValidateResponse.Types.ErrorType.DuplicateStepImplementation;
                errorMessage = string.Format("Multiple step implementations found for : {0}", stepToValidate);
            }

            return GetStepValidateResponseMessage(isValid, errorType, errorMessage, suggestion);
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

        private static StepValidateResponse GetStepValidateResponseMessage(bool isValid,
            StepValidateResponse.Types.ErrorType errorType, string errorMessage, string suggestion)
        {
            return new StepValidateResponse
            {
                ErrorMessage = errorMessage,
                IsValid = isValid,
                ErrorType = errorType,
                Suggestion = suggestion
            };
        }
    }
}