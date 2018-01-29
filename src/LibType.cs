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

using System.Collections.Generic;

namespace Gauge.Dotnet
{
    public enum LibType
    {
        Table,
        Step,
        BeforeSuite,
        BeforeSpec,
        BeforeScenario,
        BeforeStep,
        AfterStep,
        AfterScenario,
        AfterSpec,
        AfterSuite,
        FilteredHookAttribute,
        TagAggregationBehaviourAttribute,
        DataStoreFactory,
        MessageCollector,
        ContinueOnFailure,
        DefaultScreenGrabber,
        DefaultClassInstanceManager
    }

    public static class LibTypeExtensions
    {
        private static readonly Dictionary<LibType, string> typeNames = new Dictionary<LibType, string>
        {
            { LibType.Table, "Gauge.CSharp.Lib.Table" },
            { LibType.Step, "Gauge.CSharp.Lib.Attribute.Step" },
            { LibType.BeforeSuite, "Gauge.CSharp.Lib.Attribute.BeforeSuite" },
            { LibType.BeforeSpec, "Gauge.CSharp.Lib.Attribute.BeforeSpec" },
            { LibType.BeforeScenario, "Gauge.CSharp.Lib.Attribute.BeforeScenario" },
            { LibType.BeforeStep, "Gauge.CSharp.Lib.Attribute.BeforeStep" },
            { LibType.AfterStep, "Gauge.CSharp.Lib.Attribute.AfterStep" },
            { LibType.AfterScenario, "Gauge.CSharp.Lib.Attribute.AfterScenario" },
            { LibType.AfterSpec, "Gauge.CSharp.Lib.Attribute.AfterSpec" },
            { LibType.AfterSuite, "Gauge.CSharp.Lib.Attribute.AfterSuite" },
            { LibType.FilteredHookAttribute, "Gauge.CSharp.Lib.Attribute.FilteredHookAttribute" },
            { LibType.TagAggregationBehaviourAttribute, "Gauge.CSharp.Lib.Attribute.TagAggregationBehaviourAttribute" },
            { LibType.DataStoreFactory, "Gauge.CSharp.Lib.DataStoreFactory" },
            { LibType.MessageCollector, "Gauge.CSharp.Lib.MessageCollector" },
            { LibType.ContinueOnFailure, "Gauge.CSharp.Lib.Attribute.ContinueOnFailure" },
            { LibType.DefaultScreenGrabber, "Gauge.CSharp.Lib.DefaultScreenGrabber" },
            { LibType.DefaultClassInstanceManager, "Gauge.CSharp.Lib.DefaultClassInstanceManager" }
        };

        public static string FullName(this LibType type)
        {
            return typeNames[type];
        }
    }
}
