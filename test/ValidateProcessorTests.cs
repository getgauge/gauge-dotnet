// Copyright 2018 ThoughtWorks, Inc.
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

using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class ValidateProcessorTests
    {
        [SetUp]
        public void Setup()
        {
            _mockStepRegistry = new Mock<IStepRegistry>();
            var mockSandBox = new Mock<ISandbox>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector));
            var mockActivatorWrapper = new Mock<IActivatorWrapper>();
            var mockTableFormatter = new Mock<ITableFormatter>();
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            _messageProcessorFactory = new MessageProcessorFactory(_mockStepRegistry.Object);
            _messageProcessorFactory.InitializeExecutionMessageHandlers(mockReflectionWrapper.Object,
                mockAssemblyLoader.Object, mockActivatorWrapper.Object, mockTableFormatter.Object, mockSandBox.Object);
        }

        private MessageProcessorFactory _messageProcessorFactory;
        private Mock<IStepRegistry> _mockStepRegistry;

        [Test]
        public void ShouldGetErrorResponseForStepValidateRequestWhenMultipleStepImplFound()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepValidateRequest);
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0
            };
            var message = new Message
            {
                MessageId = 1,
                MessageType = Message.Types.MessageType.StepValidateRequest,
                StepValidateRequest = request
            };
            _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
            _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(true);

            var response = messageProcessor.Process(message);

            Assert.AreEqual(false, response.StepValidateResponse.IsValid);
            Assert.AreEqual(StepValidateResponse.Types.ErrorType.DuplicateStepImplementation,
                response.StepValidateResponse.ErrorType);
            Assert.AreEqual("Multiple step implementations found for : step_text_1",
                response.StepValidateResponse.ErrorMessage);
        }

        [Test]
        public void ShouldGetErrorResponseForStepValidateRequestWhennNoImplFound()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepValidateRequest);
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0
            };
            var message = new Message
            {
                MessageId = 1,
                MessageType = Message.Types.MessageType.StepValidateRequest,
                StepValidateRequest = request
            };

            var response = messageProcessor.Process(message);

            Assert.AreEqual(false, response.StepValidateResponse.IsValid);
            Assert.AreEqual(StepValidateResponse.Types.ErrorType.StepImplementationNotFound,
                response.StepValidateResponse.ErrorType);
            StringAssert.Contains("No implementation found for : step_text_1.",
                response.StepValidateResponse.ErrorMessage);
        }


        [Test]
        public void ShouldGetVaildResponseForStepValidateRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepValidateRequest);
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0
            };
            var message = new Message
            {
                MessageId = 1,
                MessageType = Message.Types.MessageType.StepValidateRequest,
                StepValidateRequest = request
            };
            _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
            _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(false);

            var response = messageProcessor.Process(message);

            Assert.AreEqual(true, response.StepValidateResponse.IsValid);
        }
    }
}