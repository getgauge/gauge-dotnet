/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Registries;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class StepValidationProcessor : IGaugeProcessor<StepValidateRequest, StepValidateResponse>
{
    private readonly IStepRegistry _stepRegistry;

    public StepValidationProcessor(IStepRegistry stepRegistry)
    {
        _stepRegistry = stepRegistry;
    }

    public Task<StepValidateResponse> Process(int stream, StepValidateRequest request)
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
            var implementations = _stepRegistry.MethodsFor(stepToValidate);
            var locations = string.Join("\n", implementations.Select(m =>
                $"  {m.ClassName}.{m.Name} in {m.FileName}:{m.Span.StartLinePosition.Line + 1}"));
            errorMessage = $"Step: {stepToValidate}\n{locations}";
        }
        return Task.FromResult(GetStepValidateResponseMessage(isValid, errorType, errorMessage, suggestion));
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