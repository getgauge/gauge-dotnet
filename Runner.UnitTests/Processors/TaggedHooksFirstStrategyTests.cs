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
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Gauge.CSharp.Runner.UnitTests.Helpers;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    [TestFixture]
    public class TaggedHooksFirstStrategyTests
    {
        [SetUp]
        public void Setup()
        {
            IHookMethod Create(string name, int aggregation = 0, params string[] tags)
            {
                var mockAssemblyLoader = new Mock<IAssemblyLoader>();
                var method = new MockMethodBuilder(mockAssemblyLoader)
                    .WithName(name)
                    .WithTagAggregation(aggregation)
                    .WithDeclaringTypeName("my.foo.type")
                    .WithFilteredHook(LibType.AfterScenario, tags)
                    .Build();

                return new HookMethod(LibType.AfterScenario, method, mockAssemblyLoader.Object);
            }


            _hookMethods = new HashSet<IHookMethod>
            {
                Create("Foo", 0, "Foo"),
                Create("Bar", 0, "Bar", "Baz"),
                Create("Baz", 1, "Foo", "Baz"),
                Create("Blah"),
                Create("Zed"),
            };
        }

        //[AfterScenario("Foo")]
        //public void Foo()
        //{
        //}

        //[AfterScenario("Bar", "Baz")]
        //public void Bar()
        //{
        //}

        //[AfterScenario("Foo", "Baz")]
        //[TagAggregationBehaviour(TagAggregation.Or)]
        //public void Baz()
        //{
        //}

        //[AfterScenario]
        //public void Blah()
        //{
        //}

        //[AfterScenario]
        //public void Zed()
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
         * After hooks should execute tagged hooks prior to untagged
         */
        private HashSet<IHookMethod> _hookMethods;

        [Test]
        public void ShouldFetchTaggedHooksInSortedOrder()
        {
            var untaggedHooks = new[] {"my.foo.type.Blah", "my.foo.type.Zed"};

            var applicableHooks = new TaggedHooksFirstStrategy()
                .GetApplicableHooks(new List<string> {"Foo"}, _hookMethods).ToArray();
            var actual = new ArraySegment<string>(applicableHooks, 2, untaggedHooks.Count());

            Assert.AreEqual(untaggedHooks, actual);
        }

        [Test]
        public void ShouldFetchUntaggedHooksAfterTaggedHooks()
        {
            var taggedHooks = new[] { "my.foo.type.Baz", "my.foo.type.Foo" };
            var untaggedHooks = new[] { "my.foo.type.Blah", "my.foo.type.Zed" };
            var expected = taggedHooks.Concat(untaggedHooks);

            var applicableHooks = new TaggedHooksFirstStrategy()
                .GetApplicableHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.AreEqual(expected, applicableHooks);
        }

        [Test]
        public void ShouldFetchUntaggedHooksInSortedOrder()
        {
            var applicableHooks = new TaggedHooksFirstStrategy()
                .GetApplicableHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.That(applicableHooks[0], Is.EqualTo("my.foo.type.Baz"));
            Assert.That(applicableHooks[1], Is.EqualTo("my.foo.type.Foo"));
        }
    }
}