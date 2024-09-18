/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using Gauge.Messages;
using Microsoft.Extensions.Configuration;

namespace Gauge.Dotnet.UnitTests.Processors;

[TestFixture]
public class ExecutionStartingProcessorTests
{
    [SetUp]
    public void Setup()
    {
        var mockHookRegistry = new Mock<IHookRegistry>();
        var mockAssemblyLoader = new Mock<IAssemblyLoader>();
        var mockMethod = new MockMethodBuilder(mockAssemblyLoader)
            .WithName("Foo")
            .WithFilteredHook(LibType.BeforeSpec)
            .Build();

        var hooks = new HashSet<IHookMethod>
        {
            new HookMethod(LibType.BeforeSpec, mockMethod, mockAssemblyLoader.Object)
        };
        mockHookRegistry.Setup(x => x.BeforeSuiteHooks).Returns(hooks);

        _mockMethodExecutor = new Mock<IExecutionOrchestrator>();
        _protoExecutionResult = new ProtoExecutionResult
        {
            ExecutionTime = 0,
            Failed = false
        };

        _mockMethodExecutor.Setup(x =>
                x.ExecuteHooks("BeforeSuite", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(), It.IsAny<int>(), It.IsAny<ExecutionInfo>()))
            .ReturnsAsync(_protoExecutionResult);
        _mockMethodExecutor.Setup(x =>
            x.GetAllPendingMessages()).Returns(_pendingMessages);
        _mockMethodExecutor.Setup(x =>
            x.GetAllPendingScreenshotFiles()).Returns(_pendingScreenshotFiles);
        var config = new ConfigurationBuilder().Build();
        _executionStartingProcessor = new ExecutionStartingProcessor(_mockMethodExecutor.Object, config);
    }

    private ExecutionStartingProcessor _executionStartingProcessor;

    private Mock<IExecutionOrchestrator> _mockMethodExecutor;
    private ProtoExecutionResult _protoExecutionResult;

    private readonly IEnumerable<string> _pendingMessages = new List<string> { "Foo", "Bar" };

    private readonly IEnumerable<string> _pendingScreenshotFiles =
        new List<string> { "screenshot.png" };

    [Test]
    public void ShouldExtendFromHooksExecutionProcessor()
    {
        AssertEx.InheritsFrom<HookExecutionProcessor, ExecutionStartingProcessor>();
        AssertEx.DoesNotInheritsFrom<TaggedHooksFirstExecutionProcessor, ExecutionStartingProcessor>();
        AssertEx.DoesNotInheritsFrom<UntaggedHooksFirstExecutionProcessor, ExecutionStartingProcessor>();
    }

    [Test]
    public void ShouldGetEmptyTagListByDefault()
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


        var tags = AssertEx.ExecuteProtectedMethod<ExecutionStartingProcessor>("GetApplicableTags", currentScenario);
        ClassicAssert.IsEmpty(tags);
    }

    [Test]
    public async Task ShouldProcessHooks()
    {
        var executionStartingRequest = new ExecutionStartingRequest();
        var result = await _executionStartingProcessor.Process(1, executionStartingRequest);

        _mockMethodExecutor.VerifyAll();
        ClassicAssert.AreEqual(result.ExecutionResult.Message, _pendingMessages);
        ClassicAssert.AreEqual(result.ExecutionResult.ScreenshotFiles, _pendingScreenshotFiles);
    }
}