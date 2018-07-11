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

using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class MessageProcessorFactoryTests
    {
        [SetUp]
        public void Setup()
        {
            var mockLoader = new Mock<IStaticLoader>();
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockLoader.Setup(l => l.GetStepRegistry()).Returns(mockStepRegistry.Object);
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector));
            var mockActivatorWrapper = new Mock<IActivatorWrapper>();
            var mockTableFormatter = new Mock<ITableFormatter>();
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            _messageProcessorFactory = new MessageProcessorFactory(mockLoader.Object);
            var mockClassInstanceManager = new Mock<object>().Object;
            _messageProcessorFactory.InitializeExecutionMessageHandlers(mockReflectionWrapper.Object,
                mockAssemblyLoader.Object, mockActivatorWrapper.Object, mockTableFormatter.Object,
                mockClassInstanceManager);
        }

        private MessageProcessorFactory _messageProcessorFactory;

        [Test]
        public void ShouldGetProcessorForCacheFileRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.CacheFileRequest);

            Assert.AreEqual(messageProcessor.GetType(), typeof(CacheFileProcessor));
        }

        [Test]
        public void ShouldGetProcessorForExecuteStep()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ExecuteStep);
            Assert.AreEqual(messageProcessor.GetType(), typeof(ExecuteStepProcessor));
        }

        [Test]
        public void ShouldGetProcessorForExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ExecutionEnding);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ExecutionEndingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForExecutionStarting()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ExecutionStarting);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ExecutionStartingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForKillProcessRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.KillProcessRequest);

            Assert.AreEqual(messageProcessor.GetType(), typeof(KillProcessProcessor));
        }

        [Test]
        public void ShouldGetProcessorForScenarioDataStoreInitRequest()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ScenarioDataStoreInit);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ScenarioDataStoreInitProcessor));
        }

        [Test]
        public void ShouldGetProcessorForScenarioExecutionEnding()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ScenarioExecutionEnding);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ScenarioExecutionEndingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForScenarioExecutionStarting()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.ScenarioExecutionStarting);

            Assert.AreEqual(messageProcessor.GetType(), typeof(ScenarioExecutionStartingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForSpecDataStoreInitRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SpecDataStoreInit);

            Assert.AreEqual(messageProcessor.GetType(), typeof(SpecDataStoreInitProcessor));
        }

        [Test]
        public void ShouldGetProcessorForSpecExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SpecExecutionEnding);

            Assert.AreEqual(messageProcessor.GetType(), typeof(SpecExecutionEndingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForSpecExecutionStarting()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SpecExecutionStarting);

            Assert.AreEqual(messageProcessor.GetType(), typeof(SpecExecutionStartingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStepExecutionEnding()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepExecutionEnding);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StepExecutionEndingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStepExecutionStarting()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepExecutionStarting);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StepExecutionStartingProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStepNameRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepNameRequest);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StepNameProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStepNamesRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepNamesRequest);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StepNamesProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStepValidateRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepValidateRequest);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StepValidationProcessor));
        }

        [Test]
        public void ShouldGetProcessorForStubImplementationRequest()
        {
            var messageProcessor =
                _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StubImplementationCodeRequest);

            Assert.AreEqual(messageProcessor.GetType(), typeof(StubImplementationCodeProcessor));
        }

        [Test]
        public void ShouldGetProcessorForSuiteDataStoreInitRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.SuiteDataStoreInit);

            Assert.AreEqual(messageProcessor.GetType(), typeof(SuiteDataStoreInitProcessor));
        }
    }
}