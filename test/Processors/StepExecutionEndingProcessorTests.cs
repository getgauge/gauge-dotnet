/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Registries;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using Gauge.Messages;
using Microsoft.Extensions.Configuration;
using static Gauge.Dotnet.Constants;

namespace Gauge.Dotnet.UnitTests.Processors;

internal class StepExecutionEndingProcessorTests
{
    private readonly IEnumerable<string> _pendingMessages = new List<string> { "Foo", "Bar" };

    private readonly IEnumerable<string> _pendingScreenshotFiles = new List<string> { "SCREENSHOT.png" };

    private Mock<IExecutionOrchestrator> _mockMethodExecutor;
    private ProtoExecutionResult _protoExecutionResult;
    private StepExecutionEndingRequest _stepExecutionEndingRequest;
    private StepExecutionEndingProcessor _stepExecutionEndingProcessor;

    [SetUp]
    public void Setup()
    {
        var mockHookRegistry = new Mock<IHookRegistry>();
        var mockAssemblyLoader = new Mock<IAssemblyLoader>();
        var mockMessageCollectorType = new Mock<Type>();
        var mockScreenshotFilesCollectorType = new Mock<Type>();

        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector))
            .Returns(mockMessageCollectorType.Object);
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ScreenshotFilesCollector))
            .Returns(mockScreenshotFilesCollectorType.Object);
        var mockMethod = new MockMethodBuilder(mockAssemblyLoader)
            .WithName("Foo")
            .WithFilteredHook(LibType.BeforeSpec)
            .Build();
        var hooks = new HashSet<IHookMethod>
        {
            new HookMethod(LibType.BeforeSpec, mockMethod, mockAssemblyLoader.Object)
        };
        mockHookRegistry.Setup(x => x.AfterStepHooks).Returns(hooks);
        _stepExecutionEndingRequest = new StepExecutionEndingRequest
        {
            CurrentExecutionInfo = new ExecutionInfo
            {
                CurrentSpec = new SpecInfo(),
                CurrentScenario = new ScenarioInfo()
            }
        };

        _mockMethodExecutor = new Mock<IExecutionOrchestrator>();
        _protoExecutionResult = new ProtoExecutionResult
        {
            ExecutionTime = 0,
            Failed = false
        };

        _mockMethodExecutor.Setup(x =>
                x.ExecuteHooks("AfterStep", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(), It.IsAny<int>(), It.IsAny<ExecutionInfo>()))
            .ReturnsAsync(_protoExecutionResult);
        _mockMethodExecutor.Setup(x =>
            x.GetAllPendingMessages()).Returns(_pendingMessages);
        _mockMethodExecutor.Setup(x =>
            x.GetAllPendingScreenshotFiles()).Returns(_pendingScreenshotFiles);
        var config = new ConfigurationBuilder().Build();
        _stepExecutionEndingProcessor = new StepExecutionEndingProcessor(_mockMethodExecutor.Object, config);
    }

    [Test]
    public void ShouldExtendFromHooksExecutionProcessor()
    {
        AssertEx.InheritsFrom<TaggedHooksFirstExecutionProcessor, StepExecutionEndingProcessor>();
    }

    [Test]
    public async Task ShouldReadPendingMessages()
    {
        var response = await _stepExecutionEndingProcessor.Process(1, _stepExecutionEndingRequest);

        ClassicAssert.True(response != null);
        ClassicAssert.True(response.ExecutionResult != null);
        ClassicAssert.AreEqual(2, response.ExecutionResult.Message.Count);
        ClassicAssert.AreEqual(1, response.ExecutionResult.ScreenshotFiles.Count);

        foreach (var pendingMessage in _pendingMessages)
            ClassicAssert.Contains(pendingMessage, response.ExecutionResult.Message.ToList());
    }

    [Test]
    public void ShouldGetTagListFromScenarioAndSpec()
    {
        var specInfo = new SpecInfo
        {
            Tags = { "foo" },
            Name = "",
            FileName = "",
            IsFailed = false
        };
        var scenarioInfo = new ScenarioInfo
        {
            Tags = { "bar" },
            Name = "",
            IsFailed = false
        };
        var currentScenario = new ExecutionInfo
        {
            CurrentScenario = scenarioInfo,
            CurrentSpec = specInfo
        };

        var tags = AssertEx.ExecuteProtectedMethod<StepExecutionEndingProcessor>("GetApplicableTags", currentScenario)
            .ToList();
        ClassicAssert.IsNotEmpty(tags);
        ClassicAssert.AreEqual(2, tags.Count);
        ClassicAssert.Contains("foo", tags);
        ClassicAssert.Contains("bar", tags);
    }

    [Test]
    public void ShouldGetTagListFromScenarioAndSpecAndIgnoreDuplicates()
    {
        var specInfo = new SpecInfo
        {
            Tags = { "foo" },
            Name = "",
            FileName = "",
            IsFailed = false
        };
        var scenarioInfo = new ScenarioInfo
        {
            Tags = { "foo" },
            Name = "",
            IsFailed = false
        };
        var currentScenario = new ExecutionInfo
        {
            CurrentScenario = scenarioInfo,
            CurrentSpec = specInfo
        };
        var currentExecutionInfo = new StepExecutionEndingRequest
        {
            CurrentExecutionInfo = currentScenario
        };

        var tags = AssertEx.ExecuteProtectedMethod<StepExecutionEndingProcessor>("GetApplicableTags", currentScenario)
            .ToList();
        ClassicAssert.IsNotEmpty(tags);
        ClassicAssert.AreEqual(1, tags.Count);
        ClassicAssert.Contains("foo", tags);
    }
}