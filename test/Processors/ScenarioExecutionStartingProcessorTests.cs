/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;
using Microsoft.Extensions.Configuration;

namespace Gauge.Dotnet.UnitTests.Processors;

internal class ScenarioExecutionStartingProcessorTests
{
    [Test]
    public void ShouldExtendFromUntaggedHooksFirstExecutionProcessor()
    {
        AssertEx.InheritsFrom<UntaggedHooksFirstExecutionProcessor, ScenarioExecutionStartingProcessor>();
    }

    [Test]
    public void ShouldGetTagListFromExecutionInfo()
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
            CurrentSpec = specInfo,
            CurrentScenario = scenarioInfo
        };
        var currentExecutionInfo = new ScenarioExecutionStartingRequest
        {
            CurrentExecutionInfo = currentScenario
        };


        var tags = AssertEx.ExecuteProtectedMethod<ScenarioExecutionStartingProcessor>("GetApplicableTags", currentScenario)
            .ToList();

        ClassicAssert.IsNotEmpty(tags);
        ClassicAssert.AreEqual(2, tags.Count);
        ClassicAssert.Contains("foo", tags);
        ClassicAssert.Contains("bar", tags);
    }

    [Test]
    public void ShouldNotFetchDuplicateTags()
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
            CurrentSpec = specInfo,
            CurrentScenario = scenarioInfo
        };


        var tags = AssertEx.ExecuteProtectedMethod<ScenarioExecutionStartingProcessor>("GetApplicableTags", currentScenario)
            .ToList();

        ClassicAssert.IsNotEmpty(tags);
        ClassicAssert.AreEqual(1, tags.Count);
        ClassicAssert.Contains("foo", tags);
    }

    [Test]
    public async Task ShouldExecuteBeforeScenarioHook()
    {
        var scenarioExecutionEndingRequest = new ScenarioExecutionStartingRequest
        {
            CurrentExecutionInfo = new ExecutionInfo
            {
                CurrentSpec = new SpecInfo(),
                CurrentScenario = new ScenarioInfo()
            }
        };

        var mockMethodExecutor = new Mock<IExecutionOrchestrator>();
        var protoExecutionResult = new ProtoExecutionResult
        {
            ExecutionTime = 0,
            Failed = false
        };

        var pendingMessages = new List<string> { "one", "two" };
        var pendingScreenshotFiles = new List<string> { "screenshot.png" };

        mockMethodExecutor.Setup(x =>
                x.ExecuteHooks("BeforeScenario", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(), It.IsAny<int>(), It.IsAny<ExecutionInfo>()))
            .ReturnsAsync(protoExecutionResult);
        mockMethodExecutor.Setup(x =>
            x.GetAllPendingMessages()).Returns(pendingMessages);
        mockMethodExecutor.Setup(x =>
            x.GetAllPendingScreenshotFiles()).Returns(pendingScreenshotFiles);
        var config = new ConfigurationBuilder().Build();

        var processor = new ScenarioExecutionStartingProcessor(mockMethodExecutor.Object, config);

        var result = await processor.Process(1, scenarioExecutionEndingRequest);
        ClassicAssert.False(result.ExecutionResult.Failed);
        ClassicAssert.AreEqual(result.ExecutionResult.Message.ToList(), pendingMessages);
        ClassicAssert.AreEqual(result.ExecutionResult.ScreenshotFiles.ToList(), pendingScreenshotFiles);
    }
}