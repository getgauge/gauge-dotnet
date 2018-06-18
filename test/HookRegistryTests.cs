// Copyright 2018 ThoughtWorks, Inc.
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


        public ISandbox SandBox;
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