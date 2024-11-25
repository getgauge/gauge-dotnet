/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using static Gauge.Dotnet.Constants;

namespace Gauge.Dotnet.UnitTests.Processors;

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
            Create("Zed")
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
        var untaggedHooks = new[] { "my.foo.type.Blah", "my.foo.type.Zed" };

        var applicableHooks = new TaggedHooksFirstStrategy()
            .GetApplicableHooks(new List<string> { "Foo" }, _hookMethods).ToArray();
        var actual = new ArraySegment<string>(applicableHooks, 2, untaggedHooks.Count());

        ClassicAssert.AreEqual(untaggedHooks, actual);
    }

    [Test]
    public void ShouldFetchUntaggedHooksAfterTaggedHooks()
    {
        var taggedHooks = new[] { "my.foo.type.Baz", "my.foo.type.Foo" };
        var untaggedHooks = new[] { "my.foo.type.Blah", "my.foo.type.Zed" };
        var expected = taggedHooks.Concat(untaggedHooks);

        var applicableHooks = new TaggedHooksFirstStrategy()
            .GetApplicableHooks(new List<string> { "Foo" }, _hookMethods).ToList();

        ClassicAssert.AreEqual(expected, applicableHooks);
    }

    [Test]
    public void ShouldFetchUntaggedHooksInSortedOrder()
    {
        var applicableHooks = new TaggedHooksFirstStrategy()
            .GetApplicableHooks(new List<string> { "Foo" }, _hookMethods).ToList();

        ClassicAssert.That(applicableHooks[0], Is.EqualTo("my.foo.type.Baz"));
        ClassicAssert.That(applicableHooks[1], Is.EqualTo("my.foo.type.Foo"));
    }
}