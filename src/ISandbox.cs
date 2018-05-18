// Copyright 2015 ThoughtWorks, Inc.
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

using System;
using System.Collections.Generic;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;

namespace Gauge.Dotnet
{
    public interface ISandbox
    {
        ExecutionResult ExecuteMethod(GaugeMethod gaugeMethod, params string[] args);
        bool TryScreenCapture(out byte[] screenShotBytes);
        List<GaugeMethod> GetStepMethods();
        IEnumerable<string> GetStepTexts(GaugeMethod gaugeMethod);
        List<string> GetAllStepTexts();
        void ClearObjectCache();
        void StartExecutionScope(string tag);
        void CloseExectionScope();
        ExecutionResult ExecuteHooks(string hookType, IHooksStrategy strategy, IList<string> applicableTags, ExecutionContext context);

        string Refactor(GaugeMethod methodInfo, IList<Tuple<int, int>> parameterPositions,
            IList<string> parametersList, string newStepValue);
    }
}