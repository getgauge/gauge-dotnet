/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System.Collections.Generic;
using static Gauge.Messages.StepPositionsResponse.Types;

namespace Gauge.Dotnet.Models
{
    public interface IStepRegistry
    {
        bool ContainsStep(string parsedStepText);
        GaugeMethod MethodFor(string parsedStepText);
        bool HasAlias(string stepText);
        string GetStepText(string parameterizedStepText);
        IEnumerable<string> GetStepTexts();
        bool HasMultipleImplementations(string parsedStepText);
        void AddStep(string stepValue, GaugeMethod stepMethod);
        void RemoveSteps(string filepath);
        IEnumerable<StepPosition> GetStepPositions(string filePath);
        bool IsFileCached(string file);
    }
}