// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Processors;
using Gauge.CSharp.Runner.Strategy;
using Gauge.Messages;
using Moq;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    public class ExecutionStartingProcessorTests
    {
        public ExecutionStartingProcessorTests()
        {
            var mockHookRegistry = new Mock<IHookRegistry>();

            var hooks = new HashSet<IHookMethod>
            {
                new HookMethod(typeof(BeforeSpec), GetType().GetMethod("Foo"))
            };
            mockHookRegistry.Setup(x => x.BeforeSuiteHooks).Returns(hooks);
            var executionEndingRequest = new ExecutionStartingRequest();
            _request = new Message
            {
                MessageId = 20,
                MessageType = Message.Types.MessageType.ExecutionEnding,
                ExecutionStartingRequest = executionEndingRequest
            };

            _mockMethodExecutor = new Mock<IMethodExecutor>();
            _protoExecutionResult = new ProtoExecutionResult
            {
                ExecutionTime = 0,
                Failed = false
            };
            _mockMethodExecutor.Setup(x => x.ExecuteHooks("BeforeSuite", It.IsAny<HooksStrategy>(), new List<string>()))
                .Returns(_protoExecutionResult);
            _executionStartingProcessor = new ExecutionStartingProcessor(_mockMethodExecutor.Object);
        }

        private ExecutionStartingProcessor _executionStartingProcessor;
        private Message _request;
        private Mock<IMethodExecutor> _mockMethodExecutor;
        private ProtoExecutionResult _protoExecutionResult;

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Foo()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        [Fact]
        public void ShouldExtendFromHooksExecutionProcessor()
        {
            AssertEx.InheritsFrom<HookExecutionProcessor, ExecutionStartingProcessor>();
            AssertEx.DoesNotInheritsFrom<TaggedHooksFirstExecutionProcessor, ExecutionStartingProcessor>();
            AssertEx.DoesNotInheritsFrom<UntaggedHooksFirstExecutionProcessor, ExecutionStartingProcessor>();
        }

        [Fact]
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
            Assert.Empty(tags);
        }

        [Fact]
        public void ShouldProcessHooks()
        {
            _executionStartingProcessor.Process(_request);
            _mockMethodExecutor.VerifyAll();
        }
    }
}