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
using System.Linq;
using System.Text.RegularExpressions;

namespace Gauge.Dotnet.Models
{
    [Serializable]
    public class StepRegistry : IStepRegistry
    {

        private Dictionary<string, List<GaugeMethod>> registry;

        public StepRegistry()
        {
            this.registry = new Dictionary<string, List<GaugeMethod>>();
        }

        public IEnumerable<string> GetStepTexts()
        {
            return registry.Values.SelectMany(methods => {
                return methods.SelectMany(method => {
                    return method.StepTexts;
                });
            });
        }

        public void AddStep(string stepValue, GaugeMethod method)
        {
            if (!registry.ContainsKey(stepValue))
            {
                registry.Add(stepValue, new List<GaugeMethod>());
            }
            registry.GetValueOrDefault(stepValue).Add(method);

        }

        public bool ContainsStep(string parsedStepText)
        {
            return registry.ContainsKey(parsedStepText);
        }

        public bool HasMultipleImplementations(string parsedStepText)
        {
            return registry[parsedStepText].Count > 1;
        }

        public void Clear()
        {
            registry = new Dictionary<string, List<GaugeMethod>>();
        }

        public GaugeMethod MethodFor(string parsedStepText)
        {
            return registry[parsedStepText][0];
        }


        public IEnumerable<string> AllSteps()
        {
            return registry.Keys;
        }

        public bool HasAlias(string stepValue)
        {
            return registry.ContainsKey(stepValue) && registry.GetValueOrDefault(stepValue).FirstOrDefault().StepTexts.Count() > 1;
        }

        public string GetStepText(string stepValue)
        {
            return registry.ContainsKey(stepValue) ? registry[stepValue][0].StepTexts.FirstOrDefault() : string.Empty;
        }
    }
}