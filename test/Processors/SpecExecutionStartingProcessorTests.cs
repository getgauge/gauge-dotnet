/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.UnitTests.Processors
{
    internal class SpecExecutionStartingProcessorTests
    {
        [Test]
        public void ShouldExtendFromUntaggedHooksFirstExecutionProcessor()
        {
            AssertEx.InheritsFrom<UntaggedHooksFirstExecutionProcessor, SpecExecutionStartingProcessor>();
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
            var executionInfo = new ExecutionInfo
            {
                CurrentSpec = specInfo
            };

            var tags = AssertEx.ExecuteProtectedMethod<SpecExecutionStartingProcessor>("GetApplicableTags", executionInfo)
                .ToList();

            ClassicAssert.IsNotEmpty(tags);
            ClassicAssert.AreEqual(1, tags.Count);
            ClassicAssert.Contains("foo", tags);
        }

        [Test]
        public async Task ShouldExecutreBeforeSpecHook()
        {
            var specExecutionStartingRequest = new SpecExecutionStartingRequest
            {
                CurrentExecutionInfo = new ExecutionInfo
                {
                    CurrentSpec = new SpecInfo()
                }
            };
            var request = specExecutionStartingRequest;


            var mockMethodExecutor = new Mock<IExecutionOrchestrator>();
            var protoExecutionResult = new ProtoExecutionResult
            {
                ExecutionTime = 0,
                Failed = false
            };
            var pendingMessages = new List<string> { "one", "two" };
            var pendingScreenshotFiles = new List<string> { "screenshot.png" };

            mockMethodExecutor.Setup(x =>
                    x.ExecuteHooks("BeforeSpec", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(),
                        It.IsAny<ExecutionInfo>()))
                .Returns(Task.FromResult(protoExecutionResult));
            mockMethodExecutor.Setup(x =>
                x.GetAllPendingMessages()).Returns(pendingMessages);
            mockMethodExecutor.Setup(x =>
                x.GetAllPendingScreenshotFiles()).Returns(pendingScreenshotFiles);

            var processor = new SpecExecutionStartingProcessor(mockMethodExecutor.Object);

            var result = await processor.Process(request);
            ClassicAssert.False(result.ExecutionResult.Failed);
            ClassicAssert.AreEqual(result.ExecutionResult.Message.ToList(), pendingMessages);
            ClassicAssert.AreEqual(result.ExecutionResult.ScreenshotFiles.ToList(), pendingScreenshotFiles);
        }
    }
}