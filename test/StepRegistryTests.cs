/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;
using Gauge.Dotnet.Registries;

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

            ClassicAssert.True(stepRegistry.LookupStep("Foo").Exists);
            ClassicAssert.True(stepRegistry.LookupStep("Bar").Exists);
        }

        [Test]
        public void ShouldGetAliasWhenExists()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("foo {}",
                    new GaugeMethod
                    {
                        StepValue = "foo {}",
                        Name = "Foo",
                        StepText = "foo <something>",
                        HasAlias = true
                    }),
                new KeyValuePair<string, GaugeMethod>("bar {}",
                    new GaugeMethod
                    {
                        StepValue = "bar {}",
                        Name = "Foo",
                        StepText = "boo <something>",
                        HasAlias = true
                    })
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);

            ClassicAssert.True(stepRegistry.HasAlias("foo {}"));
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

            ClassicAssert.AreEqual(allSteps.Count, 2);
            ClassicAssert.True(allSteps.Contains("Foo"));
            ClassicAssert.True(allSteps.Contains("Bar"));
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

            ClassicAssert.AreEqual(stepRegistry.GetStepText("random"), string.Empty);
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

            var method = stepRegistry.LookupStep("Foo").Methods[0];

            ClassicAssert.AreEqual(method.Name, "Foo");
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
                        StepText = "Foo <something>"
                    }),
                new KeyValuePair<string, GaugeMethod>("Bar", new GaugeMethod {Name = "Bar"})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);


            ClassicAssert.AreEqual(stepRegistry.GetStepText("Foo {}"), "Foo <something>");
        }

        [Test]
        public void ShouldNotHaveAliasWhenSingleStepTextIsDefined()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo",
                    new GaugeMethod {Name = "Foo", StepText = "Foo"}),
                new KeyValuePair<string, GaugeMethod>("Bar",
                    new GaugeMethod {Name = "Bar", StepText = "Bar"})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);

            ClassicAssert.False(stepRegistry.HasAlias("Foo"));
            ClassicAssert.False(stepRegistry.HasAlias("Bar"));
        }

        [Test]
        public void ShouldRemoveStepsDefinedInAGivenFile()
        {
            var methods = new[]
            {
                new KeyValuePair<string, GaugeMethod>("Foo",
                    new GaugeMethod {Name = "Foo", StepText = "Foo", FileName = "Foo.cs"}),
                new KeyValuePair<string, GaugeMethod>("Bar",
                    new GaugeMethod {Name = "Bar", StepText = "Bar", FileName = "Bar.cs"})
            };
            var stepRegistry = new StepRegistry();
            foreach (var pair in methods)
                stepRegistry.AddStep(pair.Key, pair.Value);

            stepRegistry.RemoveSteps("Foo.cs");
            ClassicAssert.False(stepRegistry.LookupStep("Foo").Exists);
        }

        [Test]
        public void ShouldCheckIfFileIsCached()
        {
            var stepRegistry = new StepRegistry();
            stepRegistry.AddStep("Foo", new GaugeMethod { Name = "Foo", StepText = "Foo", FileName = "Foo.cs" });

            ClassicAssert.True(stepRegistry.IsFileCached("Foo.cs"));
            ClassicAssert.False(stepRegistry.IsFileCached("Bar.cs"));
        }

        [Test]
        public void ShouldDetectGenuineDuplicateSteps()
        {
            var stepRegistry = new StepRegistry();
            var method1 = new GaugeMethod { Name = "Click", ClassName = "ActionsA", StepText = "Click <element>", FileName = "ActionsA.cs" };
            var method2 = new GaugeMethod { Name = "Click", ClassName = "ActionsB", StepText = "Click <element>", FileName = "ActionsB.cs" };

            stepRegistry.AddStep("Click {}", method1);
            stepRegistry.AddStep("Click {}", method2);

            ClassicAssert.True(stepRegistry.LookupStep("Click {}").HasMultipleImplementations);
        }

        [Test]
        public void ShouldNotContainStepPositionForExternalSteps()
        {
            var stepRegistry = new StepRegistry();
            stepRegistry.AddStep("Foo", new GaugeMethod { Name = "Foo", StepText = "Foo", FileName = "foo.cs" });
            stepRegistry.AddStep("Bar", new GaugeMethod { Name = "Bar", StepText = "Bar", IsExternal = true });

            var positions = stepRegistry.GetStepPositions("foo.cs");

            ClassicAssert.True(positions.Count() == 1);
            ClassicAssert.AreNotEqual(positions.First().StepValue, "Bar");
        }
    }
}