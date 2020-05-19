/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using Gauge.Dotnet.UnitTests.Processors.Stubs;
using Gauge.Dotnet.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Processors
{
    [TestFixture]
    public class HookExecutionProcessorTests
    {
        [SetUp]
        public void Setup()
        {
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();

            mockFooMethod = new MockMethodBuilder(mockAssemblyLoader)
                .WithName("FooMethod")
                .WithFilteredHook(LibType.BeforeScenario, "Foo")
                .Build();
            mockBarMethod = new MockMethodBuilder(mockAssemblyLoader)
                .WithName("BarMethod")
                .WithFilteredHook(LibType.BeforeScenario, "Bar", "Baz")
                .Build();
            mockBazMethod = new MockMethodBuilder(mockAssemblyLoader)
                .WithName("BazMethod")
                .WithFilteredHook(LibType.BeforeScenario, "Foo", "Baz")
                .WithTagAggregation(1)
                .Build();
            mockBlahMethod = new MockMethodBuilder(mockAssemblyLoader)
                .WithName("BlahMethod")
                .WithFilteredHook(LibType.BeforeScenario)
                .Build();

            _hookMethods = new List<IHookMethod>
            {
                new HookMethod(LibType.BeforeScenario, mockFooMethod, mockAssemblyLoader.Object),
                new HookMethod(LibType.BeforeScenario, mockBarMethod, mockAssemblyLoader.Object),
                new HookMethod(LibType.BeforeScenario, mockBazMethod, mockAssemblyLoader.Object),
                new HookMethod(LibType.BeforeScenario, mockBlahMethod, mockAssemblyLoader.Object)
            };
        }

        //[BeforeScenario("Foo")]
        //public void Foo()
        //{
        //}

        //[BeforeScenario("Bar", "Baz")]
        //public void Bar()
        //{
        //}

        //[BeforeScenario("Foo", "Baz")]
        //[TagAggregationBehaviour(TagAggregation.Or)]
        //public void Baz()
        //{
        //}

        //[BeforeScenario]
        //public void Blah()
        //{
        //}


        /*
         * untagged hooks are executed for all.
         * Tags     | Methods
         * Foo      | Foo, Baz
         * Bar      | NONE
         * Baz      | Baz
         * Bar, Baz | Bar, Baz
         * Foo, Baz | Baz
         */

        private IList<IHookMethod> _hookMethods;
        private MethodInfo mockFooMethod;
        private MethodInfo mockBarMethod;
        private MethodInfo mockBazMethod;
        private MethodInfo mockBlahMethod;

        [Test]
        public void ShouldAllowMultipleHooksInaMethod()
        {
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockMethod = new MockMethodBuilder(mockAssemblyLoader)
                .WithName("MultipleHookMethod")
                .WithFilteredHook(LibType.BeforeScenario)
                .WithFilteredHook(LibType.BeforeSpec)
                .Build();


            var beforeScenarioHook = new HookMethod(LibType.BeforeScenario, mockMethod, mockAssemblyLoader.Object);
            Assert.AreEqual("MultipleHookMethod", beforeScenarioHook.Method);

            var beforeSpecHook = new HookMethod(LibType.BeforeSpec, mockMethod, mockAssemblyLoader.Object);
            Assert.AreEqual("MultipleHookMethod", beforeSpecHook.Method);
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Baz", "Bar"}, _hookMethods)
                .ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            Assert.Contains(mockBarMethod.FullyQuallifiedName(), applicableHooks);
            Assert.Contains(mockBazMethod.FullyQuallifiedName(), applicableHooks);
        }

        [Test]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks =
                new HooksStrategy().GetTaggedHooks(new List<string> {"Baz", "Foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            Assert.Contains(mockFooMethod.FullyQuallifiedName(), applicableHooks);
            Assert.Contains(mockBazMethod.FullyQuallifiedName(), applicableHooks);
        }

        [Test]
        public void ShouldFetchAllHooksWhenNoTagsSpecified()
        {
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string>(), _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count());
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTags()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(2, applicableHooks.Count);
            Assert.Contains(mockFooMethod.FullyQuallifiedName(), applicableHooks);
        }

        [Test]
        public void ShouldFetchAllHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Bar"}, _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.IsEmpty(applicableHooks);
        }

        [Test]
        public void ShouldFetchAnyHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Baz"}, _hookMethods).ToList();

            Assert.IsNotNull(applicableHooks);
            Assert.AreEqual(1, applicableHooks.Count);
            Assert.Contains(mockBazMethod.FullyQuallifiedName(), applicableHooks);
        }

        [Test]
        public void ShouldNotFetchAnyTaggedHooksWhenTagsAreASuperSet()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Bar", "Blah"}, _hookMethods);

            Assert.IsNotNull(applicableHooks);
            Assert.IsEmpty(applicableHooks);
        }

        [Test]
        public void ShouldUseDefaultHooksStrategy()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            assemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector));
            var reflectionWrapper = new Mock<IReflectionWrapper>();
            var hooksStrategy = new TestHooksExecutionProcessor(null)
                .GetHooksStrategy();

            Assert.IsInstanceOf<HooksStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldUseTaggedHooksFirstStrategy()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            assemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector));
            var reflectionWrapper = new Mock<IReflectionWrapper>();
            var hooksStrategy =
                new TestTaggedHooksFirstExecutionProcessor(null)
                    .GetHooksStrategy();

            Assert.IsInstanceOf<TaggedHooksFirstStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldUseUntaggedHooksFirstStrategy()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            assemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector));
            var reflectionWrapper = new Mock<IReflectionWrapper>();
            var hooksStrategy =
                new TestUntaggedHooksFirstExecutionProcessor(null)
                    .GetHooksStrategy();

            Assert.IsInstanceOf<UntaggedHooksFirstStrategy>(hooksStrategy);
        }
    }
}