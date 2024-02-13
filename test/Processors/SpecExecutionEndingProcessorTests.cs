/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.UnitTests.Processors
{
    internal class SpecExecutionEndingProcessorTests
    {
        [Test]
        public void ShouldExtendFromTaggedHooksFirstExecutionProcessor()
        {
            AssertEx.InheritsFrom<TaggedHooksFirstExecutionProcessor, SpecExecutionEndingProcessor>();
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

            var tags = AssertEx.ExecuteProtectedMethod<SpecExecutionEndingProcessor>("GetApplicableTags", executionInfo)
                .ToList();

            ClassicAssert.IsNotEmpty(tags);
            ClassicAssert.AreEqual(1, tags.Count);
            ClassicAssert.Contains("foo", tags);
        }

        [Test]
        public void ShouldExecuteBeforeSpecHook()
        {
            var request = new SpecExecutionEndingRequest
            {
                CurrentExecutionInfo = new ExecutionInfo
                {
                    CurrentSpec = new SpecInfo()
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
                    x.ExecuteHooks("AfterSpec", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(),
                        It.IsAny<ExecutionInfo>()))
                .Returns(protoExecutionResult);
            mockMethodExecutor.Setup(x =>
                x.GetAllPendingMessages()).Returns(pendingMessages);
            mockMethodExecutor.Setup(x =>
                x.GetAllPendingScreenshotFiles()).Returns(pendingScreenshotFiles);
            var processor = new SpecExecutionEndingProcessor(mockMethodExecutor.Object);

            var result = processor.Process(request);
            ClassicAssert.False(result.ExecutionResult.Failed);
            ClassicAssert.AreEqual(result.ExecutionResult.Message.ToList(), pendingMessages);
            ClassicAssert.AreEqual(result.ExecutionResult.ScreenshotFiles.ToList(), pendingScreenshotFiles);
        }
    }
}