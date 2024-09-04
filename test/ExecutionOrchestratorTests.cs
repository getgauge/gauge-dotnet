/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet.UnitTests;

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
    public async Task ShouldExecuteHooks()
    {
        var pendingMessages = new List<string> { "Foo", "Bar" };
        var pendingScreenshots = new List<string> { "screenshot.png" };
        var executionResult = new ExecutionResult { Success = true };
        var mockReflectionWrapper = new Mock<IReflectionWrapper>();
        var mockAssemblyLoader = new Mock<IAssemblyLoader>();
        var mockClassInstanceManager = new Mock<object>().Object;
        var hooksStrategy = new HooksStrategy();
        var mockHookExecuter = new Mock<IHookExecutor>();
        mockHookExecuter.Setup(m =>
            m.Execute("hooks", hooksStrategy, new List<string>(), It.IsAny<int>(), It.IsAny<ExecutionInfo>())
        ).ReturnsAsync(executionResult).Verifiable();
        var mockStepExecuter = new Mock<IStepExecutor>();
        var reflectionWrapper = mockReflectionWrapper.Object;
        var mockType = new Mock<Type>().Object;
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ScreenshotFilesCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManager);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingMessages", It.IsAny<BindingFlags>()))
            .Returns(pendingMessages);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingScreenshotFiles", It.IsAny<BindingFlags>()))
            .Returns(pendingScreenshots);
        var assemblyLoader = mockAssemblyLoader.Object;
        var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            mockHookExecuter.Object, mockStepExecuter.Object);

        var result = await executionOrchestrator.ExecuteHooks("hooks", hooksStrategy, new List<string>(), 1, It.IsAny<ExecutionInfo>());

        mockHookExecuter.VerifyAll();
        ClassicAssert.False(result.Failed);
    }

    [Test]
    public async Task ShouldExecuteHooksAndNotTakeScreenshotOnFailureWhenDisabled()
    {
        var pendingMessages = new List<string> { "Foo", "Bar" };
        var pendingScreenshots = new List<string> { "screenshot.png" };
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
        var mockHookExecuter = new Mock<IHookExecutor>();
        var mockStepExecuter = new Mock<IStepExecutor>();
        var reflectionWrapper = mockReflectionWrapper.Object;
        var assemblyLoader = mockAssemblyLoader.Object;
        var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            mockHookExecuter.Object, mockStepExecuter.Object);
        mockHookExecuter.Setup(executor =>
            executor.Execute("hooks", hooksStrategy, new List<string>(), It.IsAny<int>(), It.IsAny<ExecutionInfo>())
        ).ReturnsAsync(executionResult).Verifiable();
        var mockType = new Mock<Type>().Object;
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ScreenshotFilesCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManager);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingMessages", It.IsAny<BindingFlags>()))
            .Returns(pendingMessages);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingScreenshotFiles", It.IsAny<BindingFlags>()))
            .Returns(pendingScreenshots);

        var screenshotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
        Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", "false");

        var result = await orchestrator.ExecuteHooks("hooks", hooksStrategy, new List<string>(), 1, It.IsAny<ExecutionInfo>());

        mockHookExecuter.VerifyAll();
        ClassicAssert.True(result.Failed);
        ClassicAssert.True(string.IsNullOrEmpty(result.FailureScreenshotFile));
        Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", screenshotEnabled);
    }

    [Test]
    public async Task ShouldExecuteMethod()
    {
        var pendingMessages = new List<string> { "Foo", "Bar" };
        var pendingScreenshots = new List<string> { "screenshot.png" };
        var gaugeMethod = new GaugeMethod { Name = "ShouldExecuteMethod", ParameterCount = 1 };
        var args = new[] { "Bar", "String" };

        var mockClassInstanceManager = new Mock<object>().Object;
        var mockReflectionWrapper = new Mock<IReflectionWrapper>();
        var mockAssemblyLoader = new Mock<IAssemblyLoader>();
        var mockHookExecuter = new Mock<IHookExecutor>();
        var mockStepExecutor = new Mock<IStepExecutor>();

        mockStepExecutor.Setup(executor => executor.Execute(gaugeMethod, It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(() => new ExecutionResult { Success = true })
            .Callback(() => Thread.Sleep(1)); // Simulate a delay in method execution

        var orchestrator = new ExecutionOrchestrator(mockReflectionWrapper.Object, mockAssemblyLoader.Object,
            mockHookExecuter.Object, mockStepExecutor.Object);

        var mockType = new Mock<Type>().Object;
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ScreenshotFilesCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManager);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingMessages", It.IsAny<BindingFlags>()))
            .Returns(pendingMessages);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingScreenshotFiles", It.IsAny<BindingFlags>()))
            .Returns(pendingScreenshots);
        var result = await orchestrator.ExecuteStep(gaugeMethod, 1, args);
        mockStepExecutor.VerifyAll();
        ClassicAssert.False(result.Failed);
        ClassicAssert.True(result.ExecutionTime > 0);
    }

    [Test]
    public async Task ShouldNotTakeScreenShotWhenDisabled()
    {
        var pendingMessages = new List<string> { "Foo", "Bar" };
        var pendingScreenshots = new List<string> { "screenshot.png" };
        var gaugeMethod = new GaugeMethod { Name = "ShouldNotTakeScreenShotWhenDisabled", ParameterCount = 1 };

        var executionResult = new ExecutionResult
        {
            Success = false,
            ExceptionMessage = "Some Error",
            StackTrace = "StackTrace"
        };
        var mockClassInstanceManager = new Mock<object>().Object;
        var mockReflectionWrapper = new Mock<IReflectionWrapper>();
        var mockAssemblyLoader = new Mock<IAssemblyLoader>();
        var mockHookExecuter = new Mock<IHookExecutor>();
        var mockStepExecutor = new Mock<IStepExecutor>();
        mockStepExecutor.Setup(executor => executor.Execute(gaugeMethod, It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(executionResult);
        var mockType = new Mock<Type>().Object;
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ScreenshotFilesCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManager);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingMessages", It.IsAny<BindingFlags>()))
            .Returns(pendingMessages);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingScreenshotFiles", It.IsAny<BindingFlags>()))
            .Returns(pendingScreenshots);

        var orchestrator = new ExecutionOrchestrator(mockReflectionWrapper.Object, mockAssemblyLoader.Object,
            mockHookExecuter.Object, mockStepExecutor.Object);

        var screenshotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
        Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", "false");

        var result = await orchestrator.ExecuteStep(gaugeMethod, 1, "Bar", "string");

        mockStepExecutor.VerifyAll();
        ClassicAssert.True(string.IsNullOrEmpty(result.FailureScreenshotFile));
        Environment.SetEnvironmentVariable("SCREENSHOT_ON_FAILURE", screenshotEnabled);
    }

    [Test]
    public async Task ShouldTakeScreenShotOnFailedExecution()
    {
        var pendingMessages = new List<string> { "Foo", "Bar" };
        var expectedScreenshot = "Testscreenshot.png";
        var pendingScreenshots = new List<string> { expectedScreenshot };
        var gaugeMethod = new GaugeMethod { Name = "ShouldExecuteMethod", ParameterCount = 1 };
        var executionResult = new ExecutionResult
        {
            Success = false,
            ExceptionMessage = "error",
            StackTrace = "stacktrace"
        };
        var mockInstance = new Mock<object>().Object;
        var mockAssemblyLoader = new Mock<IAssemblyLoader>();
        var mockReflectionWrapper = new Mock<IReflectionWrapper>();
        var mockHookExecuter = new Mock<IHookExecutor>();
        var mockStepExecutor = new Mock<IStepExecutor>();
        mockStepExecutor.Setup(executor => executor.Execute(gaugeMethod, It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(executionResult).Verifiable();
        var mockType = new Mock<Type>().Object;
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ScreenshotFilesCollector)).Returns(mockType);
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockInstance);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingMessages", It.IsAny<BindingFlags>()))
            .Returns(pendingMessages);
        mockReflectionWrapper.Setup(x =>
                x.InvokeMethod(mockType, null, "GetAllPendingScreenshotFiles", It.IsAny<BindingFlags>()))
            .Returns(pendingScreenshots);

        var orchestrator = new ExecutionOrchestrator(mockReflectionWrapper.Object, mockAssemblyLoader.Object,
            mockHookExecuter.Object, mockStepExecutor.Object);

        var result = await orchestrator.ExecuteStep(gaugeMethod, 1, "Bar", "String");
        mockStepExecutor.VerifyAll();


        ClassicAssert.True(result.Failed);
        ClassicAssert.AreEqual(expectedScreenshot, result.FailureScreenshotFile);
    }
}