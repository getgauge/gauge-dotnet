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
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    public class SandboxHookExecutionTests
    {
        private static readonly IEnumerable<string> HookTypes = Hooks.Keys;
        private IList<string> _applicableTags;
        private string _gaugeProjectRootEnv;
        private HashSet<IHookMethod> _hookMethods;
        private Mock<IAssemblyLoader> _mockAssemblyLoader;
        private Mock<IReflectionWrapper> reflectionWrapper;
        private Mock<IHookRegistry> _mockHookRegistry;
        private Mock<IHooksStrategy> _mockStrategy;
        private Mock<IActivatorWrapper> activatorWrapper;
        private Mock<object> mockInstance;
        private Mock<MethodInfo> mockHookMethodInfo;

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
        const string MethodName = "DummyHook";

        [SetUp]
        public void Setup()
        {
            _gaugeProjectRootEnv = Environment.GetEnvironmentVariable("GAUGE_PROJECT_ROOT");
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());
            _mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockAssembly = new Mock<Assembly>();
            _mockAssemblyLoader.Setup(loader => loader.AssembliesReferencingGaugeLib)
                .Returns(new List<Assembly> { mockAssembly.Object });
            _mockAssemblyLoader.Setup(loader => loader.ScreengrabberType);

            var instanceManagerType = new Mock<Type>();
            var mockInstanceManager = new Mock<object>();
            var mockInitMethod = new Mock<MethodInfo>();
            _mockAssemblyLoader.Setup(loader => loader.ClassInstanceManagerType)
                .Returns(instanceManagerType.Object);

            activatorWrapper = new Mock<IActivatorWrapper>();
            activatorWrapper.Setup(x => x.CreateInstance(instanceManagerType.Object))
                .Returns(mockInstanceManager.Object);

            reflectionWrapper = new Mock<IReflectionWrapper>();
            reflectionWrapper.Setup(x => x.InvokeMethod(instanceManagerType.Object, mockInstanceManager.Object, "Initialize", It.IsAny<object[]>()));
            _mockHookRegistry = new Mock<IHookRegistry>();
            var mockHookMethod = new Mock<IHookMethod>();
            var mockHookMethodType = new Mock<Type>();
            mockHookMethod.Setup(method => method.Method).Returns(MethodName);
            _hookMethods = new HashSet<IHookMethod> { mockHookMethod.Object };
            _mockStrategy = new Mock<IHooksStrategy>();
            _applicableTags = Enumerable.Empty<string>().ToList();
            _mockStrategy.Setup(strategy => strategy.GetApplicableHooks(_applicableTags, _hookMethods))
                .Returns(new[] { MethodName });

            var mockType = new Mock<Type>();
            mockInstance = new Mock<object>();
            mockHookMethodInfo = new Mock<MethodInfo>();
            mockHookMethodInfo.Setup(x => x.DeclaringType).Returns(mockType.Object);

            reflectionWrapper.Setup(x => x.InvokeMethod(instanceManagerType.Object, mockInstanceManager.Object, "Get", mockType.Object))
                .Returns(mockInstance.Object);
        }

        [Test]
        [TestCaseSource("HookTypes")]
        public void ShouldExecuteHook(string hookType)
        {
            var expression = Hooks[hookType];
            _mockHookRegistry.Setup(registry => registry.MethodFor(MethodName))
                .Returns(mockHookMethodInfo.Object);
            _mockHookRegistry.Setup(expression).Returns(_hookMethods).Verifiable();

            reflectionWrapper.Setup(x => x.Invoke(mockHookMethodInfo.Object, mockInstance.Object, new object[] { }))
                .Verifiable();

            var sandbox = new Sandbox(_mockAssemblyLoader.Object, _mockHookRegistry.Object, activatorWrapper.Object, reflectionWrapper.Object);
            var executionResult = sandbox.ExecuteHooks(hookType, _mockStrategy.Object, _applicableTags, new ExecutionContext());

            Assert.IsTrue(executionResult.Success, executionResult.ExceptionMessage);
            _mockHookRegistry.VerifyAll();
            reflectionWrapper.VerifyAll();
        }

        [Test]
        [TestCaseSource("HookTypes")]
        public void ShouldExecuteHookAndReportFailureOnException(string hookType)
        {
            var expression = Hooks[hookType];
            _mockHookRegistry.Setup(registry => registry.MethodFor(MethodName))
                .Returns(mockHookMethodInfo.Object);
            _mockHookRegistry.Setup(expression).Returns(_hookMethods).Verifiable();

            reflectionWrapper.Setup(x => x.Invoke(mockHookMethodInfo.Object, mockInstance.Object, new object[] { }))
                .Throws(new Exception("foo"))
                .Verifiable();

            var sandbox = new Sandbox(_mockAssemblyLoader.Object, _mockHookRegistry.Object, activatorWrapper.Object, reflectionWrapper.Object);
            var executionResult = sandbox.ExecuteHooks(hookType, _mockStrategy.Object, _applicableTags, new ExecutionContext());

            Assert.False(executionResult.Success);
            Assert.AreEqual("foo", executionResult.ExceptionMessage);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _gaugeProjectRootEnv);
        }
    }
}