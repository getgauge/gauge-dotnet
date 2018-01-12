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
using System.Reflection;
using Gauge.CSharp.Runner.Extensions;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Gauge.CSharp.Runner.UnitTests.Processors.Stubs;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    class GaugeMethodBuilder
    {
        private class TestFilteredHook
        {
            public string[] FilterTags { get; set; }
        }

        private class TestTagAggregation
        {
            public int TagAggregation { get; set; }
        }

        public static MethodInfo GetMockHookMethod(Mock<IAssemblyLoader> mockAssemblyLoader, LibType[] hooks, string name, Dictionary<string, string> methodParams, string[] tags, int? aggregation, bool filtered = true)
        {
            var mockMethod = new Mock<MethodInfo>();
            var mockFilteredHook = new Mock<Type>();
            mockAssemblyLoader.Setup(x => x.GetLibType(LibType.FilteredHookAttribute)).Returns(filtered ? mockFilteredHook.Object : null);

            var mockHookType = new Mock<Type>();
            var methodAttributes = new List<object>();
            mockHookType.Setup(x => x.IsSubclassOf(mockFilteredHook.Object)).Returns(true);
            var hookAttribute = new TestFilteredHook{ FilterTags = tags };
            mockHookType.Setup(x => x.IsInstanceOfType(It.IsAny<TestFilteredHook>())).Returns(true);
            foreach (var hook in hooks)
            {
                mockAssemblyLoader.Setup(a => a.GetLibType(hook)).Returns(mockHookType.Object);
            }

            methodAttributes.Add(hookAttribute);

            var mockTagAggregationBehaviourType = new Mock<Type>();
            if (aggregation.HasValue)
            {
                var aggregationAttribute = new TestTagAggregation{ TagAggregation = aggregation.Value };
                methodAttributes.Add(aggregationAttribute);
            }
            mockTagAggregationBehaviourType.Setup(x => x.IsInstanceOfType(It.IsAny<TestTagAggregation>())).Returns(true);
            mockAssemblyLoader.Setup(a => a.GetLibType(LibType.TagAggregationBehaviourAttribute)).Returns(mockTagAggregationBehaviourType.Object);
            mockMethod.Setup(x => x.GetParameters()).Returns(GetParameters(methodParams));
            mockMethod.Setup(x => x.DeclaringType).Returns(default(Type));
            mockMethod.Setup(x => x.GetCustomAttributes(false)).Returns(methodAttributes.ToArray());
            mockMethod.Setup(x => x.Name).Returns(name);
            return mockMethod.Object;
        }

        static ParameterInfo[] GetParameters(Dictionary<string, string> methodParams)
        {
            if (methodParams is null)
                return Enumerable.Empty<ParameterInfo>().ToArray();
            return methodParams.Select(p =>
            {
                var pInfo = new Mock<ParameterInfo>();
                var pType = new Mock<Type>();
                pType.Setup(x => x.Name).Returns(p.Key);
                pInfo.Setup(x => x.ParameterType).Returns(pType.Object);
                pInfo.Setup(x => x.Name).Returns(p.Value);
                return pInfo.Object;
            }).ToArray();
        }
    }

    [TestFixture]
    public class HookExecutionProcessorTests
    {

        [SetUp]
        public void Setup()
        {
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();

            mockFooMethod = GaugeMethodBuilder.GetMockHookMethod(mockAssemblyLoader, new[] { LibType.BeforeScenario }, "FooMethod", null, new[] { "Foo" }, null);
            mockBarMethod = GaugeMethodBuilder.GetMockHookMethod(mockAssemblyLoader, new[] { LibType.BeforeScenario }, "BarMethod", null, new[] { "Bar", "Baz" }, null);
            mockBazMethod = GaugeMethodBuilder.GetMockHookMethod(mockAssemblyLoader, new[] { LibType.BeforeScenario }, "BazMethod", null, new[] { "Foo", "Baz" }, 1);
            mockBlahMethod = GaugeMethodBuilder.GetMockHookMethod(mockAssemblyLoader, new[] { LibType.BeforeScenario }, "BlahMethod", null, null, null); 

            _hookMethods = new List<IHookMethod>
            {
                new HookMethod(LibType.BeforeScenario, mockFooMethod, mockAssemblyLoader.Object),
                new HookMethod(LibType.BeforeScenario, mockBarMethod, mockAssemblyLoader.Object),
                new HookMethod(LibType.BeforeScenario, mockBazMethod, mockAssemblyLoader.Object),
                new HookMethod(LibType.BeforeScenario, mockBlahMethod, mockAssemblyLoader.Object),
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
            var mockMethod = GaugeMethodBuilder.GetMockHookMethod(mockAssemblyLoader, new[] { LibType.BeforeScenario, LibType.BeforeSpec }, "MultipleHookMethod", null, null, null);


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
            var hooksStrategy = new TestHooksExecutionProcessor(null, assemblyLoader.Object).GetHooksStrategy();

            Assert.IsInstanceOf<HooksStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldUseTaggedHooksFirstStrategy()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            assemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector));
            var hooksStrategy = new TestTaggedHooksFirstExecutionProcessor(null, assemblyLoader.Object).GetHooksStrategy();

            Assert.IsInstanceOf<TaggedHooksFirstStrategy>(hooksStrategy);
        }

        [Test]
        public void ShouldUseUntaggedHooksFirstStrategy()
        {
            var assemblyLoader = new Mock<IAssemblyLoader>();
            assemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector));
            var hooksStrategy = new TestUntaggedHooksFirstExecutionProcessor(null, assemblyLoader.Object).GetHooksStrategy();

            Assert.IsInstanceOf<UntaggedHooksFirstStrategy>(hooksStrategy);
        }
    }
}