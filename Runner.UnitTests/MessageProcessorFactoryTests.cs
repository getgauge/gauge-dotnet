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

using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using Moq;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests
{
    public class MessageProcessorFactoryTests
    {
        public MessageProcessorFactoryTests()
        {
            var mockMethodScanner = new Mock<IMethodScanner>();
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockMethodScanner.Setup(x => x.GetStepRegistry()).Returns(mockStepRegistry.Object);
            var mockSandBox = new Mock<ISandbox>();
            _messageProcessorFactory = new MessageProcessorFactory(mockMethodScanner.Object, mockSandBox.Object);
        }

        private MessageProcessorFactory _messageProcessorFactory;

        [Fact]
        public void ShouldGetProcessorForExecuteStep()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ExecuteStep);

            Assert.IsType<ExecuteStepProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ExecutionEnding);

            Assert.IsType<ExecutionEndingProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForExecutionStarting()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ExecutionStarting);

            Assert.IsType<ExecutionStartingProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForKillProcessRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.KillProcessRequest);

            Assert.IsType<KillProcessProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForScenarioDataStoreInitRequest()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ScenarioDataStoreInit);

            Assert.IsType<ScenarioDataStoreInitProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForScenarioExecutionEnding()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ScenarioExecutionEnding);

            Assert.IsType<ScenarioExecutionEndingProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForScenarioExecutionStarting()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ScenarioExecutionStarting);

            Assert.IsType<ScenarioExecutionStartingProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForSpecDataStoreInitRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SpecDataStoreInit);

            Assert.IsType<SpecDataStoreInitProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForSpecExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SpecExecutionEnding);

            Assert.IsType<SpecExecutionEndingProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForSpecExecutionStarting()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SpecExecutionStarting);

            Assert.IsType<SpecExecutionStartingProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForStepExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepExecutionEnding);

            Assert.IsType<StepExecutionEndingProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForStepExecutionStarting()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepExecutionStarting);

            Assert.IsType<StepExecutionStartingProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForStepNamesRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepNamesRequest);

            Assert.IsType<StepNamesProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForStepValidateRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepValidateRequest);

            Assert.IsType<StepValidationProcessor>(messageProcessor);
        }

        [Fact]
        public void ShouldGetProcessorForSuiteDataStoreInitRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SuiteDataStoreInit);

            Assert.IsType<SuiteDataStoreInitProcessor>(messageProcessor);
        }
    }
}