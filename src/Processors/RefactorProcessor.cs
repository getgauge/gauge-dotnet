/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;
using Gauge.Dotnet.Refactoring;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class RefactorProcessor : IGaugeProcessor<RefactorRequest, RefactorResponse>
{
    private readonly IStepRegistry _stepRegistry;

    public RefactorProcessor(IStepRegistry stepRegistry)
    {
        _stepRegistry = stepRegistry;
    }

    public async Task<RefactorResponse> Process(int stream, RefactorRequest request)
    {
        var newStep = request.NewStepValue;

        var newStepValue = newStep.ParameterizedStepValue;
        var parameterPositions = request.ParamPositions
            .Select(position => new Tuple<int, int>(position.OldPosition, position.NewPosition)).ToList();

        var response = new RefactorResponse();
        try
        {
            var gaugeMethod = GetGaugeMethod(request.OldStepValue);
            if (gaugeMethod.HasAlias) throw new Exception("Steps with aliases can not be refactored.");

            var fileChanges = RefactorHelper.Refactor(gaugeMethod, parameterPositions, newStep.Parameters.ToList(),
                newStepValue);

            if (request.SaveChanges)
                await File.WriteAllTextAsync(fileChanges.FileName, fileChanges.FileContent);

            response.Success = true;
            response.FilesChanged.Add(gaugeMethod.FileName);
            response.FileChanges.Add(ConvertToProtoFileChanges(fileChanges));
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


        return response;
    }

    private static FileChanges ConvertToProtoFileChanges(RefactoringChange fileChanges)
    {
        var chages = new FileChanges
        {
            FileName = fileChanges.FileName
        };
        foreach (var fileChangesDiff in fileChanges.Diffs)
            chages.Diffs.Add(new TextDiff
            {
                Content = fileChangesDiff.Content,
                Span = new Span
                {
                    Start = fileChangesDiff.Range.Start.Line,
                    StartChar = fileChangesDiff.Range.Start.Character,
                    End = fileChangesDiff.Range.End.Line,
                    EndChar = fileChangesDiff.Range.End.Character
                }
            });

        return chages;
    }

    private GaugeMethod GetGaugeMethod(ProtoStepValue stepValue)
    {
        if (_stepRegistry.HasMultipleImplementations(stepValue.StepValue))
            throw new Exception(string.Format("Multiple step implementations found for : {0}",
                stepValue.ParameterizedStepValue));
        return _stepRegistry.MethodFor(stepValue.StepValue);
    }
}