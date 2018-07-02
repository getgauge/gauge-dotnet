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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Castle.Core.Internal;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using Moq;
using NUnit.Framework;
using ExecutionContext = Gauge.CSharp.Lib.ExecutionContext;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class ExecutionOrchestratorTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT",
                Directory.GetDirectoryRoot(Assembly.GetExecutingAssembly().Location));
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        [Test]
        public void ShouldExecuteHooks()
        {
            var executionResult = new ExecutionResult {Success = true};
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockActivationWrapper = new Mock<IActivatorWrapper>();
            var mockClassInstanceManager = new Mock<object>().Object;
            var hooksStrategy = new HooksStrategy();
            var mockHookExecuter = new Mock<IHookExecutor>();
            mockHookExecuter.Setup(m =>
                m.Execute("hooks", hooksStrategy, new List<string>(), It.IsAny<ExecutionContext>())
            ).Returns(executionResult).Verifiable();
            var mockStepExecuter = new Mock<IStepExecutor>();
            var reflectionWrapper = mockReflectionWrapper.Object;
            var assemblyLoader = mockAssemblyLoader.Object;
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, mockActivationWrapper.Object,
                mockClassInstanceManager, mockHookExecuter.Object, mockStepExecuter.Object);


            var result = executionOrchestrator.ExecuteHooks("hooks", hooksStrategy, new List<string>(),
                It.IsAny<ExecutionContext>());

            mockHookExecuter.VerifyAll();
            Assert.False(result.Failed);
        }

        [Test]
        public void ShouldExecuteHooksAndNotTakeScreenshotOnFailureWhenDisabled()
        {
            var hooksStrategy = new HooksStrategy();
            var executionResult = new ExecutionResult
            {
                Success = false,
                ExceptionMessage = "Some Error",
                StackTrace = "StackTrace"
            };
            var mockClassInstanceManager = new Mock<object>().Object;
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockActivationWrapper = new Mock<IActivatorWrapper>();
            var mockHookExecuter = new Mock<IHookExecutor>();
            var mockStepExecuter = new Mock<IStepExecutor>();
            var reflectionWrapper = mockReflectionWrapper.Object;
            var assemblyLoader = mockAssemblyLoader.Object;
            var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, mockActivationWrapper.Object,
                mockClassInstanceManager,
                mockHookExecuter.Object, mockStepExecuter.Object);
            mockHookExecuter.Setup(executor =>
                executor.Execute("hooks", hooksStrategy, new List<string>(), It.IsAny<ExecutionContext>())
            ).Returns(executionResult).Verifiable();

            var screenshotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
            Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", "false");

            var result = orchestrator.ExecuteHooks("hooks", hooksStrategy, new List<string>(),
                new ExecutionContext());

            mockHookExecuter.VerifyAll();
            Assert.True(result.Failed);
            Assert.True(result.ScreenShot.IsNullOrEmpty());
            Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", screenshotEnabled);
        }

        [Test]
        public void ShouldExecuteMethod()
        {
            var gaugeMethod = new GaugeMethod {Name = "ShouldExecuteMethod", ParameterCount = 1};
            var args = new[] {"Bar", "String"};

            var mockClassInstanceManager = new Mock<object>().Object;
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockActivationWrapper = new Mock<IActivatorWrapper>();
            var mockHookExecuter = new Mock<IHookExecutor>();
            var mockStepExecutor = new Mock<IStepExecutor>();

            mockStepExecutor.Setup(executor => executor.Execute(gaugeMethod, It.IsAny<string[]>()))
                .Returns(() => new ExecutionResult {Success = true})
                .Callback(() => Thread.Sleep(1)); // Simulate a delay in method execution

            var orchestrator = new ExecutionOrchestrator(mockReflectionWrapper.Object, mockAssemblyLoader.Object,
                mockActivationWrapper.Object, mockClassInstanceManager,
                mockHookExecuter.Object, mockStepExecutor.Object);
            var result = orchestrator.ExecuteStep(gaugeMethod, args);
            mockStepExecutor.VerifyAll();
            Assert.False(result.Failed);
            Assert.True(result.ExecutionTime > 0);
        }

        [Test]
        public void ShouldNotTakeScreenShotWhenDisabled()
        {
            var gaugeMethod = new GaugeMethod {Name = "ShouldNotTakeScreenShotWhenDisabled", ParameterCount = 1};

            var executionResult = new ExecutionResult
            {
                Success = false,
                ExceptionMessage = "Some Error",
                StackTrace = "StackTrace"
            };
            var mockClassInstanceManager = new Mock<object>().Object;
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockActivationWrapper = new Mock<IActivatorWrapper>();
            var mockHookExecuter = new Mock<IHookExecutor>();
            var mockStepExecutor = new Mock<IStepExecutor>();
            mockStepExecutor.Setup(executor => executor.Execute(gaugeMethod, It.IsAny<string[]>()))
                .Returns(executionResult);

            var orchestrator = new ExecutionOrchestrator(mockReflectionWrapper.Object, mockAssemblyLoader.Object,
                mockActivationWrapper.Object, mockClassInstanceManager,
                mockHookExecuter.Object, mockStepExecutor.Object);

            var screenshotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
            Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", "false");

            var result = orchestrator.ExecuteStep(gaugeMethod, "Bar", "string");

            mockStepExecutor.VerifyAll();
            Assert.True(result.ScreenShot.IsNullOrEmpty());
            Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", screenshotEnabled);
        }

        [Test]
        public void ShouldTakeScreenShotOnFailedExecution()
        {
            var gaugeMethod = new GaugeMethod {Name = "ShouldExecuteMethod", ParameterCount = 1};
            var executionResult = new ExecutionResult
            {
                Success = false,
                ExceptionMessage = "error",
                StackTrace = "stacktrace"
            };
            var expectedScreenshot = Encoding.UTF8.GetBytes("TestScreenshot");

            var type = new Mock<Type>().Object;
            var mockInstance = new Mock<object>().Object;
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            mockAssemblyLoader.Setup(x => x.ScreengrabberType).Returns(type);

            var mockActivationWrapper = new Mock<IActivatorWrapper>();
            mockActivationWrapper.Setup(x => x.CreateInstance(type)).Returns(mockInstance);

            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            mockReflectionWrapper
                .Setup(x => x.InvokeMethod(type, mockInstance, "TakeScreenShot"))
                .Returns(expectedScreenshot);

            var mockHookExecuter = new Mock<IHookExecutor>();
            var mockStepExecutor = new Mock<IStepExecutor>();
            mockStepExecutor.Setup(executor => executor.Execute(gaugeMethod, It.IsAny<string[]>()))
                .Returns(executionResult).Verifiable();

            var orchestrator = new ExecutionOrchestrator(mockReflectionWrapper.Object, mockAssemblyLoader.Object,
                mockActivationWrapper.Object, mockInstance,
                mockHookExecuter.Object, mockStepExecutor.Object);

            var result = orchestrator.ExecuteStep(gaugeMethod, "Bar", "String");
            mockStepExecutor.VerifyAll();

            Assert.True(result.Failed);
            Assert.AreEqual(expectedScreenshot, result.ScreenShot[0]);
        }
    }
}