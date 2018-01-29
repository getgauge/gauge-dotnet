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
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Processors
{
    [TestFixture]
    public class UntaggedHooksFirstStrategyTests
    {
        [SetUp]
        public void Setup()
        {
            IHookMethod Create(string name, int aggregation=0, params string[] tags)
            {
                var mockAssemblyLoader = new Mock<IAssemblyLoader>();
                var method = new MockMethodBuilder(mockAssemblyLoader)
                    .WithName(name)
                    .WithDeclaringTypeName("my.foo.type")
                    .WithTagAggregation(aggregation)
                    .WithFilteredHook(LibType.AfterScenario, tags)
                    .Build();

                return new HookMethod(LibType.AfterScenario, method, mockAssemblyLoader.Object);
            }


            _hookMethods = new HashSet<IHookMethod>
            {
                Create("Foo", 0, "Foo"),
                Create("Bar", 0, "Foo", "Bar"),
                Create("Zed", 1),
                Create("Blah"),
                Create("Baz"),
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

        //[AfterScenario()]
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
        public void ShouldFetchTaggedHooksAfterUntaggedHooks()
        {
            var applicableHooks = new UntaggedHooksFirstStrategy()
                .GetApplicableHooks(new List<string> { "Foo" }, _hookMethods).ToList();

            var expectedMethods = new[] {
                "my.foo.type.Baz",
                "my.foo.type.Blah",
                "my.foo.type.Zed",
                "my.foo.type.Foo"};


            Assert.AreEqual(expectedMethods, applicableHooks);
        }

        [Test]
        public void ShouldFetchTaggedHooksInSortedOrder()
        {
            var applicableHooks = new UntaggedHooksFirstStrategy()
                .GetApplicableHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            var expectedMethods = new[] {
                "my.foo.type.Baz",
                "my.foo.type.Blah",
                "my.foo.type.Zed",
                "my.foo.type.Foo"};

            Assert.AreEqual(expectedMethods, applicableHooks);
        }

        [Test]
        public void ShouldFetchUntaggedHooksInSortedOrder()
        {
            var applicableHooks = new UntaggedHooksFirstStrategy()
                .GetApplicableHooks(new List<string> {"Foo"}, _hookMethods).ToList();

            Assert.AreEqual(applicableHooks[2], "my.foo.type.Zed");
            Assert.AreEqual(applicableHooks[3], "my.foo.type.Foo");
        }
    }
}