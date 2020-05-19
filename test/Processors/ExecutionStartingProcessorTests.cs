/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Text;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Processors
{
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
                    x.ExecuteHooks("BeforeSuite", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(),
                        It.IsAny<ExecutionContext>()))
                .Returns(_protoExecutionResult);
            _mockMethodExecutor.Setup(x =>
                x.GetAllPendingMessages()).Returns(_pendingMessages);
            _mockMethodExecutor.Setup(x =>
                x.GetAllPendingScreenshotFiles()).Returns(_pendingScreenshotFiles);
            _executionStartingProcessor = new ExecutionStartingProcessor(_mockMethodExecutor.Object);
        }

        private ExecutionStartingProcessor _executionStartingProcessor;

        private Mock<IExecutionOrchestrator> _mockMethodExecutor;
        private ProtoExecutionResult _protoExecutionResult;

        private readonly IEnumerable<string> _pendingMessages = new List<string> { "Foo", "Bar" };

        private readonly IEnumerable<string> _pendingScreenshotFiles =
            new List<string> { "screenshot.png"};

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
            Assert.IsEmpty(tags);
        }

        [Test]
        public void ShouldProcessHooks()
        {
            var executionStartingRequest = new ExecutionStartingRequest();
            var result = _executionStartingProcessor.Process(executionStartingRequest);

            _mockMethodExecutor.VerifyAll();
            Assert.AreEqual(result.ExecutionResult.Message, _pendingMessages);
            Assert.AreEqual(result.ExecutionResult.ScreenshotFiles, _pendingScreenshotFiles);
        }
    }
}