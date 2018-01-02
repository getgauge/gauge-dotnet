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
using System.Linq;
using Gauge.CSharp.Runner.Models;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests
{
    public class StepRegistryTests
    {
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Foo()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Bar()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [Fact]
        public void ShouldContainMethodForStepDefined()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry(methods, null, null);

            Assert.True(stepRegistry.ContainsStep("Foo"));
            Assert.True(stepRegistry.ContainsStep("Bar"));
        }

        [Fact]
        public void ShouldGetAliasWhenExists()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("FooAlias", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry(methods, null,
                new Dictionary<string, bool> {{"Foo", true}, {"FooAlias", true}});

            Assert.True(stepRegistry.HasAlias("Foo"));
            Assert.True(stepRegistry.HasAlias("FooAlias"));
        }

        [Fact]
        public void ShouldGetAllSteps()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry(methods, null, null);
            var allSteps = stepRegistry.AllSteps().ToList();

            Assert.Equal(2, allSteps.Count);
            Assert.Contains("Foo", allSteps);
            Assert.Contains("Bar", allSteps);
        }

        [Fact]
        public void ShouldGetEmptyStepTextForInvalidParameterizedStepText()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepTextMap = new Dictionary<string, string> {{"foo_parameterized", "Foo"}};

            var stepRegistry = new StepRegistry(methods, stepTextMap, null);

            Assert.Equal(stepRegistry.GetStepText("random"), string.Empty);
        }

        [Fact]
        public void ShouldGetMethodForStep()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry(methods, null, null);
            var method = stepRegistry.MethodFor("Foo");

            Assert.Equal("Foo", method.Name);
        }

        [Fact]
        public void ShouldGetStepTextFromParameterizedStepText()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepTextMap = new Dictionary<string, string> {{"foo_parameterized", "Foo"}};

            var stepRegistry = new StepRegistry(methods, stepTextMap, null);

            Assert.Equal("Foo", stepRegistry.GetStepText("foo_parameterized"));
        }

        [Fact]
        public void ShouldNotHaveAliasWhenSingleStepTextIsDefined()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry(methods, null, new Dictionary<string, bool>());

            Assert.False(stepRegistry.HasAlias("Foo"));
            Assert.False(stepRegistry.HasAlias("Bar"));
        }
    }
}