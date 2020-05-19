/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.UnitTests.Helpers;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class HookRegistryTests
    {
        [SetUp]
        public void Setup()
        {
            _mockAssemblyScanner = new Mock<IAssemblyLoader>();
            var types = new[]
            {
                LibType.BeforeScenario, LibType.AfterScenario, LibType.BeforeSpec, LibType.AfterSpec,
                LibType.BeforeStep, LibType.AfterStep, LibType.BeforeSuite, LibType.AfterSuite
            };
            foreach (var type in types)
            {
                var methodInfos = new List<MethodInfo>
                {
                    new MockMethodBuilder(_mockAssemblyScanner)
                        .WithName($"{type}Hook")
                        .WithFilteredHook(type)
                        .WithDeclaringTypeName("my.foo.type")
                        .Build()
                };
                _mockAssemblyScanner.Setup(scanner => scanner.GetMethods(type)).Returns(methodInfos);
            }

            _hookRegistry = new HookRegistry(_mockAssemblyScanner.Object);
        }


        private HookRegistry _hookRegistry;
        private Mock<IAssemblyLoader> _mockAssemblyScanner;

        [Test]
        public void ShoulddGetAfterScenarioHook()
        {
            var expectedMethods = new[] {"my.foo.type.AfterScenarioHook"};
            var hooks = _hookRegistry.AfterScenarioHooks.Select(mi => mi.Method);

            Assert.AreEqual(expectedMethods, hooks);
        }

        [Test]
        public void ShouldGetAfterSpecHook()
        {
            var expectedMethods = new[] {"my.foo.type.AfterSpecHook"};
            var hooks = _hookRegistry.AfterSpecHooks.Select(mi => mi.Method);

            Assert.AreEqual(expectedMethods, hooks);
        }

        [Test]
        public void ShouldGetAfterStepHook()
        {
            var expectedMethods = new[] {"my.foo.type.AfterStepHook"};
            var hooks = _hookRegistry.AfterStepHooks.Select(mi => mi.Method);

            Assert.AreEqual(expectedMethods, hooks);
        }

        [Test]
        public void ShouldGetAfterSuiteHook()
        {
            var expectedMethods = new[] {"my.foo.type.AfterSuiteHook"};
            var hooks = _hookRegistry.AfterSuiteHooks.Select(mi => mi.Method);

            Assert.AreEqual(expectedMethods, hooks);
        }

        [Test]
        public void ShouldGetBeforeScenarioHook()
        {
            var expectedMethods = new[] {"my.foo.type.BeforeScenarioHook"};
            var hooks = _hookRegistry.BeforeScenarioHooks.Select(mi => mi.Method);

            Assert.AreEqual(expectedMethods, hooks);
        }

        [Test]
        public void ShouldGetBeforeSpecHook()
        {
            var expectedMethods = new[] {"my.foo.type.BeforeSpecHook"};
            var hooks = _hookRegistry.BeforeSpecHooks.Select(mi => mi.Method);

            Assert.AreEqual(expectedMethods, hooks);
        }

        [Test]
        public void ShouldGetBeforeStepHook()
        {
            var expectedMethods = new[] {"my.foo.type.BeforeStepHook"};
            var hooks = _hookRegistry.BeforeStepHooks.Select(mi => mi.Method);

            Assert.AreEqual(expectedMethods, hooks);
        }

        [Test]
        public void ShouldGetBeforeSuiteHook()
        {
            var expectedMethods = new[] {"my.foo.type.BeforeSuiteHook"};
            var hooks = _hookRegistry.BeforeSuiteHooks.Select(mi => mi.Method);

            Assert.AreEqual(expectedMethods, hooks);
        }
    }
}