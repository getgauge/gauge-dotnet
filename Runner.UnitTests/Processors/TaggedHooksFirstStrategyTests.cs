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
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Extensions;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    public class TaggedHooksFirstStrategyTests
    {
        public TaggedHooksFirstStrategyTests()
        {
            _hookMethods = new HashSet<HookMethod>
            {
                new HookMethod(typeof(AfterScenario), GetType().GetMethod("Foo")),
                new HookMethod(typeof(AfterScenario), GetType().GetMethod("Bar")),
                new HookMethod(typeof(AfterScenario), GetType().GetMethod("Zed")),
                new HookMethod(typeof(AfterScenario), GetType().GetMethod("Blah")),
                new HookMethod(typeof(AfterScenario), GetType().GetMethod("Baz"))
            };
        }

        [AfterScenario("Foo")]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Foo()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [AfterScenario("Bar", "Baz")]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Bar()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [AfterScenario("Foo", "Baz")]
        [TagAggregationBehaviour(TagAggregation.Or)]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Baz()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [AfterScenario]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Blah()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [AfterScenario]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Zed()
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
         * After hooks should execute tagged hooks prior to untagged
         */
        private HashSet<HookMethod> _hookMethods;


        [Fact]
        public void ShouldFetchTaggedHooksInSortedOrder()
        {
            var untaggedHooks = new[] {"Blah", "Zed"}.Select(s => GetType().GetMethod(s).FullyQuallifiedName());

            var applicableHooks = new TaggedHooksFirstStrategy()
                .GetApplicableHooks(new List<string> {"Foo"}, _hookMethods).ToArray();
            var actual = new ArraySegment<string>(applicableHooks, 2, untaggedHooks.Count());

            Assert.Equal(untaggedHooks, actual);
        }

        [Fact]
        public void ShouldFetchUntaggedHooksAfterTaggedHooks()
        {
            var taggedHooks = new[] {"Baz", "Foo"};
            var untaggedHooks = new[] {"Blah", "Zed"};
            var expected = taggedHooks.Concat(untaggedHooks).Select(s => GetType().GetMethod(s).FullyQuallifiedName());

            var applicableHooks = new TaggedHooksFirstStrategy()
                .GetApplicableHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.Equal(expected, applicableHooks);
        }


        [Fact]
        public void ShouldFetchUntaggedHooksInSortedOrder()
        {
            var applicableHooks = new TaggedHooksFirstStrategy()
                .GetApplicableHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.Equal(applicableHooks[0], GetType().GetMethod("Baz").FullyQuallifiedName());
            Assert.Equal(applicableHooks[1], GetType().GetMethod("Foo").FullyQuallifiedName());
        }
    }
}