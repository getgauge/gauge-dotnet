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

[TestFixture]
public class ExecutionEndingProcessorTests
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
        mockHookRegistry.Setup(x => x.AfterSuiteHooks).Returns(hooks);
        var executionEndingRequest = new ExecutionEndingRequest
        {
            CurrentExecutionInfo = new ExecutionInfo
            {
                CurrentSpec = new SpecInfo(),
                CurrentScenario = new ScenarioInfo()
            }
        };
        _request = executionEndingRequest;

        _mockMethodExecutor = new Mock<IExecutionOrchestrator>();
        _protoExecutionResult = new ProtoExecutionResult
        {
            ExecutionTime = 0,
            Failed = false
        };
        _mockMethodExecutor.Setup(x =>
                x.ExecuteHooks("AfterSuite", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(), It.IsAny<int>(), It.IsAny<ExecutionInfo>()))
            .ReturnsAsync(_protoExecutionResult);
        _mockMethodExecutor.Setup(x =>
            x.GetAllPendingMessages()).Returns(_pendingMessages);
        _mockMethodExecutor.Setup(x =>
            x.GetAllPendingScreenshotFiles()).Returns(_pendingScreenshotFiles);
        var config = new ConfigurationBuilder().Build();
        _executionEndingProcessor = new ExecutionEndingProcessor(_mockMethodExecutor.Object, config);
    }

    private ExecutionEndingProcessor _executionEndingProcessor;
    private ExecutionEndingRequest _request;
    private Mock<IExecutionOrchestrator> _mockMethodExecutor;
    private ProtoExecutionResult _protoExecutionResult;
    private readonly IEnumerable<string> _pendingMessages = new List<string> { "Foo", "Bar" };

    private readonly IEnumerable<string> _pendingScreenshotFiles =
        new List<string> { "screenshot.png" };

    [Test]
    public void ShouldExtendFromHooksExecutionProcessor()
    {
        AssertEx.InheritsFrom<HookExecutionProcessor, ExecutionEndingProcessor>();
        AssertEx.DoesNotInheritsFrom<TaggedHooksFirstExecutionProcessor, ExecutionEndingProcessor>();
        AssertEx.DoesNotInheritsFrom<UntaggedHooksFirstExecutionProcessor, ExecutionEndingProcessor>();
    }

    [Test]
    public void ShouldGetEmptyTagListByDefault()
    {
        var tags = AssertEx.ExecuteProtectedMethod<ExecutionEndingProcessor>("GetApplicableTags", _request.CurrentExecutionInfo);
        ClassicAssert.IsEmpty(tags);
    }

    [Test]
    public async Task ShouldProcessHooks()
    {
        var result = await _executionEndingProcessor.Process(1, _request);
        _mockMethodExecutor.VerifyAll();
        ClassicAssert.AreEqual(result.ExecutionResult.Message, _pendingMessages);
        ClassicAssert.AreEqual(result.ExecutionResult.ScreenshotFiles, _pendingScreenshotFiles);
    }


}