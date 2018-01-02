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
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Extensions;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Gauge.CSharp.Runner.UnitTests.Processors.Stubs;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    public class HookExecutionProcessorTests
    {
        public HookExecutionProcessorTests()
        {
            _hookMethods = new List<IHookMethod>
            {
                new HookMethod(typeof(BeforeScenario), GetType().GetMethod("Foo")),
                new HookMethod(typeof(BeforeScenario), GetType().GetMethod("Bar")),
                new HookMethod(typeof(BeforeScenario), GetType().GetMethod("Baz")),
                new HookMethod(typeof(BeforeScenario), GetType().GetMethod("Blah"))
            };
        }

        [BeforeScenario("Foo")]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Foo()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [BeforeScenario("Bar", "Baz")]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Bar()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [BeforeScenario("Foo", "Baz")]
        [TagAggregationBehaviour(TagAggregation.Or)]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Baz()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [BeforeScenario]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Blah()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [BeforeSpec]
        [BeforeScenario]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void MultiHook()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

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

        [Fact]
        public void ShouldAllowMultipleHooksInaMethod()
        {
            var expected = GetType().GetMethod("MultiHook").FullyQuallifiedName();
            var beforeScenarioHook =
                new HookMethod(typeof(BeforeScenario), GetType().GetMethod("MultiHook"));
            Assert.Equal(expected, beforeScenarioHook.Method);

            var beforeSpecHook = new HookMethod(typeof(BeforeSpec), GetType().GetMethod("MultiHook"));
            Assert.Equal(expected, beforeSpecHook.Method);
        }

        [Fact]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Baz", "Bar"}, _hookMethods)
                .ToList();

            Assert.NotNull(applicableHooks);
            Assert.Equal(2, applicableHooks.Count);
            Assert.Contains(GetType().GetMethod("Bar").FullyQuallifiedName(), applicableHooks);
            Assert.Contains(GetType().GetMethod("Baz").FullyQuallifiedName(), applicableHooks);
        }

        [Fact]
        public void ShouldFetchAHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks =
                new HooksStrategy().GetTaggedHooks(new List<string> {"Baz", "Foo"}, _hookMethods).ToList();

            Assert.NotNull(applicableHooks);
            Assert.Equal(2, applicableHooks.Count);
            Assert.Contains(GetType().GetMethod("Foo").FullyQuallifiedName(), applicableHooks);
            Assert.Contains(GetType().GetMethod("Baz").FullyQuallifiedName(), applicableHooks);
        }

        [Fact]
        public void ShouldFetchAllHooksWhenNoTagsSpecified()
        {
            var applicableHooks = new HooksStrategy().GetApplicableHooks(new List<string>(), _hookMethods);

            Assert.NotNull(applicableHooks);
            Assert.Single(applicableHooks);
        }

        [Fact]
        public void ShouldFetchAllHooksWithSpecifiedTags()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.NotNull(applicableHooks);
            Assert.Equal(2, applicableHooks.Count);
            Assert.Contains(GetType().GetMethod("Foo").FullyQuallifiedName(), applicableHooks);
            Assert.Contains(GetType().GetMethod("Baz").FullyQuallifiedName(), applicableHooks);
        }

        [Fact]
        public void ShouldFetchAllHooksWithSpecifiedTagsWhenDoingAnd()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Bar"}, _hookMethods);

            Assert.NotNull(applicableHooks);
            Assert.Empty(applicableHooks);
        }

        [Fact]
        public void ShouldFetchAnyHooksWithSpecifiedTagsWhenDoingOr()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Baz"}, _hookMethods).ToList();

            Assert.NotNull(applicableHooks);
            Assert.Single(applicableHooks);
            Assert.Contains(GetType().GetMethod("Baz").FullyQuallifiedName(), applicableHooks);
        }

        [Fact]
        public void ShouldNotFetchAnyTaggedHooksWhenTagsAreASuperSet()
        {
            var applicableHooks = new HooksStrategy().GetTaggedHooks(new List<string> {"Bar", "Blah"}, _hookMethods);

            Assert.NotNull(applicableHooks);
            Assert.Empty(applicableHooks);
        }

        [Fact]
        public void ShouldUseDefaultHooksStrategy()
        {
            var hooksStrategy = new TestHooksExecutionProcessor().GetHooksStrategy();

            Assert.IsAssignableFrom<HooksStrategy>(hooksStrategy);
        }

        [Fact]
        public void ShouldUseTaggedHooksFirstStrategy()
        {
            var hooksStrategy = new TestTaggedHooksFirstExecutionProcessor().GetHooksStrategy();

            Assert.IsAssignableFrom<TaggedHooksFirstStrategy>(hooksStrategy);
        }

        [Fact]
        public void ShouldUseUntaggedHooksFirstStrategy()
        {
            var hooksStrategy = new TestUntaggedHooksFirstExecutionProcessor().GetHooksStrategy();

            Assert.IsAssignableFrom<UntaggedHooksFirstStrategy>(hooksStrategy);
        }
    }
}