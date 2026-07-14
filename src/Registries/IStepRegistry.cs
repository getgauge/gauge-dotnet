/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Models;
using static Gauge.Messages.StepPositionsResponse.Types;

namespace Gauge.Dotnet.Registries;

public interface IStepRegistry
{
    bool HasAlias(string stepText);
    string GetStepText(string parameterizedStepText);
    IEnumerable<string> GetStepTexts();
    StepLookupResult LookupStep(string parsedStepText);
    void AddStep(string stepValue, GaugeMethod stepMethod);
    void ReplaceSteps(string filepath, IReadOnlyList<(string stepValue, GaugeMethod method)> newSteps);
    void RemoveSteps(string filepath);
    IEnumerable<StepPosition> GetStepPositions(string filePath);
    bool IsFileCached(string file);
    int Count { get; }
}