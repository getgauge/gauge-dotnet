﻿/*----------------------------------------------------------------
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

internal class ScenarioExecutionEndingProcessorTests
{
    [Test]
    public void ShouldExtendFromTaggedHooksFirstExecutionProcessor()
    {
        AssertEx.InheritsFrom<TaggedHooksFirstExecutionProcessor, ScenarioExecutionEndingProcessor>();
    }

    [Test]
    public void ShouldGetTagListFromSpecAndScenario()
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
        var currentExecutionInfo = new ScenarioExecutionEndingRequest
        {
            CurrentExecutionInfo = currentScenario
        };
        var tags = AssertEx.ExecuteProtectedMethod<ScenarioExecutionEndingProcessor>("GetApplicableTags", currentScenario)
            .ToList();

        ClassicAssert.IsNotEmpty(tags);
        ClassicAssert.AreEqual(2, tags.Count);
        ClassicAssert.Contains("foo", tags);
        ClassicAssert.Contains("bar", tags);
    }

    [Test]
    public void ShouldNotGetDuplicateTags()
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


        var tags = AssertEx.ExecuteProtectedMethod<ScenarioExecutionEndingProcessor>("GetApplicableTags", currentScenario)
            .ToList();

        ClassicAssert.IsNotEmpty(tags);
        ClassicAssert.AreEqual(1, tags.Count);
        ClassicAssert.Contains("foo", tags);
    }

    [Test]
    public async Task ShouldExecutreBeforeScenarioHook()
    {
        var scenarioExecutionStartingRequest = new ScenarioExecutionEndingRequest
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
                x.ExecuteHooks("AfterScenario", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(), It.IsAny<int>(), It.IsAny<ExecutionInfo>()))
            .ReturnsAsync(protoExecutionResult);
        mockMethodExecutor.Setup(x =>
            x.GetAllPendingMessages()).Returns(pendingMessages);
        mockMethodExecutor.Setup(x =>
            x.GetAllPendingScreenshotFiles()).Returns(pendingScreenshotFiles);
        var config = new ConfigurationBuilder().Build();

        var processor = new ScenarioExecutionEndingProcessor(mockMethodExecutor.Object, config);

        var result = await processor.Process(1, scenarioExecutionStartingRequest);
        ClassicAssert.False(result.ExecutionResult.Failed);
        ClassicAssert.AreEqual(result.ExecutionResult.Message.ToList(), pendingMessages);
        ClassicAssert.AreEqual(result.ExecutionResult.ScreenshotFiles.ToList(), pendingScreenshotFiles);
    }
}