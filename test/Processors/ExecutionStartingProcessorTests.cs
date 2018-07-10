// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-Dotnet.
//
// Gauge-Dotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-Dotnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-Dotnet.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using Gauge.Dotnet.Wrappers;
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
            var executionEndingRequest = new ExecutionStartingRequest();
            _request = new Message
            {
                MessageId = 20,
                MessageType = Message.Types.MessageType.ExecutionEnding,
                ExecutionStartingRequest = executionEndingRequest
            };

            _mockMethodExecutor = new Mock<IExecutionOrchestrator>();
            _protoExecutionResult = new ProtoExecutionResult
            {
                ExecutionTime = 0,
                Failed = false
            };
            _mockMethodExecutor.Setup(x => x.ExecuteHooks("BeforeSuite", It.IsAny<HooksStrategy>(), new List<string>(),
                    It.IsAny<ExecutionContext>()))
                .Returns(_protoExecutionResult);
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            _executionStartingProcessor = new ExecutionStartingProcessor(_mockMethodExecutor.Object,
                mockAssemblyLoader.Object, mockReflectionWrapper.Object);
        }

        private ExecutionStartingProcessor _executionStartingProcessor;
        private Message _request;
        private Mock<IExecutionOrchestrator> _mockMethodExecutor;
        private ProtoExecutionResult _protoExecutionResult;

        public void Foo()
        {
        }

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
                Tags = {"foo"},
                Name = "",
                FileName = "",
                IsFailed = false
            };
            var scenarioInfo = new ScenarioInfo
            {
                Tags = {"bar"},
                Name = "",
                IsFailed = false
            };
            var currentScenario = new ExecutionInfo
            {
                CurrentScenario = scenarioInfo,
                CurrentSpec = specInfo
            };
            var currentExecutionInfo = new ScenarioExecutionStartingRequest
            {
                CurrentExecutionInfo = currentScenario
            };
            var message = new Message
            {
                ScenarioExecutionStartingRequest = currentExecutionInfo,
                MessageType = Message.Types.MessageType.ScenarioExecutionStarting,
                MessageId = 0
            };

            var tags = AssertEx.ExecuteProtectedMethod<ExecutionStartingProcessor>("GetApplicableTags", message);
            Assert.IsEmpty(tags);
        }

        [Test]
        public void ShouldProcessHooks()
        {
            _executionStartingProcessor.Process(_request);
            _mockMethodExecutor.VerifyAll();
        }
    }
}