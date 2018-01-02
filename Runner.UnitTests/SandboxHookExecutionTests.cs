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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Strategy;
using Gauge.CSharp.Runner.Wrappers;
using Moq;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests
{
    public class SandboxHookExecutionTests
    {
        public static readonly IEnumerable<object[]> HookTypes = Hooks.Keys.Cast<object[]>();
        private IList<string> _applicableTags;
        private string _gaugeProjectRootEnv;
        private HashSet<IHookMethod> _hookMethods;
        private Mock<IAssemblyLoader> _mockAssemblyLoader;
        private Mock<IHookRegistry> _mockHookRegistry;
        private Mock<IHooksStrategy> _mockStrategy;

        private static Dictionary<string, Expression<Func<IHookRegistry, HashSet<IHookMethod>>>> Hooks
        {
            get
            {
                return new Dictionary<string, Expression<Func<IHookRegistry, HashSet<IHookMethod>>>>
                {
                    {"BeforeSuite", x => x.BeforeSuiteHooks},
                    {"BeforeSpec", x => x.BeforeSpecHooks},
                    {"BeforeScenario", x => x.BeforeScenarioHooks},
                    {"BeforeStep", x => x.BeforeStepHooks},
                    {"AfterStep", x => x.AfterStepHooks},
                    {"AfterScenario", x => x.AfterScenarioHooks},
                    {"AfterSpec", x => x.AfterSpecHooks},
                    {"AfterSuite", x => x.AfterSuiteHooks}
                };
            }
        }

        public SandboxHookExecutionTests()
        {
            _gaugeProjectRootEnv = Environment.GetEnvironmentVariable("GAUGE_PROJECT_ROOT");
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());
            _mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockAssembly = new Mock<TestAssembly>();
            _mockAssemblyLoader.Setup(loader => loader.AssembliesReferencingGaugeLib)
                .Returns(new List<Assembly> {mockAssembly.Object});
            _mockAssemblyLoader.Setup(loader => loader.ScreengrabberTypes)
                .Returns(new List<Type> {typeof(DefaultScreenGrabber)});
            _mockAssemblyLoader.Setup(loader => loader.ClassInstanceManagerTypes)
                .Returns(new List<Type> {typeof(DefaultClassInstanceManager)});
            _mockHookRegistry = new Mock<IHookRegistry>();
            var mockHookMethod = new Mock<IHookMethod>();
            mockHookMethod.Setup(method => method.Method).Returns("DummyHook");
            _hookMethods = new HashSet<IHookMethod> {mockHookMethod.Object};
            _mockStrategy = new Mock<IHooksStrategy>();
            _applicableTags = Enumerable.Empty<string>().ToList();
            _mockStrategy.Setup(strategy => strategy.GetApplicableHooks(_applicableTags, _hookMethods))
                .Returns(new[] {"DummyHook"});
        }

        [Theory]
        [MemberData(nameof(HookTypes))]
        public void ShouldExecuteHook(string hookType)
        {
            var expression = Hooks[hookType];
            _mockHookRegistry.Setup(registry => registry.MethodFor("DummyHook"))
                .Returns(GetType().GetMethod("DummyHook"));
            _mockHookRegistry.Setup(expression).Returns(_hookMethods).Verifiable();

            var sandbox = new Sandbox(_mockAssemblyLoader.Object, _mockHookRegistry.Object);
            var executionResult = sandbox.ExecuteHooks(hookType, _mockStrategy.Object, _applicableTags);

            Assert.True(executionResult.Success);
            _mockHookRegistry.VerifyAll();
        }

        [Theory]
        [MemberData(nameof(HookTypes))]
        public void ShouldExecuteHookAndReportFailureOnException(string hookType)
        {
            var expression = Hooks[hookType];
            _mockHookRegistry.Setup(registry => registry.MethodFor("DummyHook"))
                .Returns(GetType().GetMethod("DummyHookThrowsException"));
            _mockHookRegistry.Setup(expression).Returns(_hookMethods).Verifiable();

            var sandbox = new Sandbox(_mockAssemblyLoader.Object, _mockHookRegistry.Object);
            var executionResult = sandbox.ExecuteHooks(hookType, _mockStrategy.Object, _applicableTags);

            Assert.False(executionResult.Success);
            Assert.Equal("foo", executionResult.ExceptionMessage);
        }

        ~SandboxHookExecutionTests()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _gaugeProjectRootEnv);
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void DummyHook()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void DummyHookThrowsException()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            throw new Exception("foo");
        }
    }
}