/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.UnitTests.Processors
{
    internal class StepExecutionStartingProcessorTests
    {
        [Test]
        public void ShouldExtendFromHooksExecutionProcessor()
        {
            AssertEx.InheritsFrom<UntaggedHooksFirstExecutionProcessor, StepExecutionStartingProcessor>();
        }

        [Test]
        public async Task ShouldClearExistingGaugeMessages()
        {
            var mockExecutionHelper = new Mock<IExecutionOrchestrator>();

            var request = new StepExecutionStartingRequest
            {
                CurrentExecutionInfo = new ExecutionInfo
                {
                    CurrentSpec = new SpecInfo(),
                    CurrentScenario = new ScenarioInfo()
                }
            };

            var protoExecutionResult = new ProtoExecutionResult { ExecutionTime = 0, Failed = false };
            mockExecutionHelper.Setup(executor =>
                    executor.ExecuteHooks(It.IsAny<string>(), It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(),
                        It.IsAny<ExecutionInfo>()))
                .Returns(Task.FromResult(protoExecutionResult));
            var hookRegistry = new Mock<IHookRegistry>();
            hookRegistry.Setup(registry => registry.BeforeStepHooks).Returns(new HashSet<IHookMethod>());

            var pendingMessages = new List<string> { "one", "two" };
            var pendingScreenshotFiles = new List<string> { "screenshot.png" };

            mockExecutionHelper.Setup(x =>
                    x.ExecuteHooks("BeforeStep", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(),
                        It.IsAny<ExecutionInfo>()))
                .Returns(Task.FromResult(protoExecutionResult));
            mockExecutionHelper.Setup(x =>
                x.GetAllPendingMessages()).Returns(pendingMessages);
            mockExecutionHelper.Setup(x =>
                x.GetAllPendingScreenshotFiles()).Returns(pendingScreenshotFiles);

            var processor = new StepExecutionStartingProcessor(mockExecutionHelper.Object);
            var result = await processor.Process(request);
            ClassicAssert.AreEqual(result.ExecutionResult.Message, pendingMessages);
            ClassicAssert.AreEqual(result.ExecutionResult.ScreenshotFiles, pendingScreenshotFiles);
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

            var tags = AssertEx.ExecuteProtectedMethod<StepExecutionStartingProcessor>("GetApplicableTags", currentScenario)
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
            var currentExecutionInfo = new StepExecutionStartingRequest
            {
                CurrentExecutionInfo = currentScenario
            };

            var tags = AssertEx.ExecuteProtectedMethod<StepExecutionStartingProcessor>("GetApplicableTags", currentScenario)
                .ToList();
            ClassicAssert.IsNotEmpty(tags);
            ClassicAssert.AreEqual(1, tags.Count);
            ClassicAssert.Contains("foo", tags);
        }
    }
}