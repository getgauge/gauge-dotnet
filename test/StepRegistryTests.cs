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
using Gauge.Dotnet.Models;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class StepRegistryTests
    {
        [Test]
        public void ShouldContainMethodForStepDefined()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);

            Assert.True(stepRegistry.ContainsStep("Foo"));
            Assert.True(stepRegistry.ContainsStep("Bar"));
        }

        [Test]
        public void ShouldGetAliasWhenExists()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo {}",
                    new GaugeMethod
                    {
                        Name = "Foo",
                        StepTexts = new List<string> {"foo <something>", "foo <somethingelse>"}
                    }),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);

            Assert.True(stepRegistry.HasAlias("Foo {}"));
        }

        [Test]
        public void ShouldGetAllSteps()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);

            var allSteps = stepRegistry.AllSteps().ToList();

            Assert.AreEqual(allSteps.Count, 2);
            Assert.True(allSteps.Contains("Foo"));
            Assert.True(allSteps.Contains("Bar"));
        }

        [Test]
        public void ShouldGetEmptyStepTextForInvalidParameterizedStepText()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);

            Assert.AreEqual(stepRegistry.GetStepText("random"), string.Empty);
        }

        [Test]
        public void ShouldGetMethodForStep()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo", new GaugeMethod {Name = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);

            var method = stepRegistry.MethodFor("Foo");

            Assert.AreEqual(method.Name, "Foo");
        }

        [Test]
        public void ShouldGetStepTextFromParameterizedStepText()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo {}",
                    new GaugeMethod
                    {
                        Name = "Foo",
                        StepValue = "foo {}",
                        StepTexts = new List<string> {"Foo <something>"}
                    }),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);


            Assert.AreEqual(stepRegistry.GetStepText("Foo {}"), "Foo <something>");
        }

        [Test]
        public void ShouldNotHaveAliasWhenSingleStepTextIsDefined()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo",
                    new GaugeMethod {Name = "Foo", StepTexts = new List<string> {"Foo"}}),
                new KeyValuePair<string, GaugeMethod>("Bar",
                    new GaugeMethod {Name = "Bar", StepTexts = new List<string> {"Bar"}})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);

            Assert.False(stepRegistry.HasAlias("Foo"));
            Assert.False(stepRegistry.HasAlias("Bar"));
        }
    }
}