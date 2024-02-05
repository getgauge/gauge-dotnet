/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


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
            if (!_stepRegistry.ContainsStep(request.StepText))
            {
                return GetStepValidateResponseMessage(false, StepValidateResponse.Types.ErrorType.StepImplementationNotFound,
                    $"No implementation found for : {request.StepText}. Full Step Text :", GetSuggestion(request.StepValue));
            }
            
            if (_stepRegistry.HasMultipleImplementations(request.StepText))
            {
                return GetStepValidateResponseMessage(false, StepValidateResponse.Types.ErrorType.DuplicateStepImplementation, 
                    $"Multiple step implementations found for : {request.StepText}", string.Empty);
            }

            if (_stepRegistry.HasAsyncVoidImplementation(request.StepText))
            {
                return GetStepValidateResponseMessage(false, StepValidateResponse.Types.ErrorType.StepImplementationNotFound,
                    string.Empty, $"Found a potential step implementation with 'async void' return for : {request.StepText}. Usage of 'async void' is discouraged (https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming#avoid-async-void). Use `async Task` instead.");
            }
            return GetStepValidateResponseMessage(true,
                StepValidateResponse.Types.ErrorType.StepImplementationNotFound, string.Empty, string.Empty);
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