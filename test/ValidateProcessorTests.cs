/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class ValidateProcessorTests
    {
        [SetUp]
        public void Setup()
        {
            _mockStepRegistry = new Mock<IStepRegistry>();

        }

        private Mock<IStepRegistry> _mockStepRegistry;

        [Test]
        public void ShouldGetErrorResponseForStepValidateRequestWhenMultipleStepImplFound()
        {
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0
            };

            _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
            _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(true);
            var processor = new StepValidationProcessor(_mockStepRegistry.Object);
            var response = processor.Process(request);

            ClassicAssert.AreEqual(false, response.IsValid);
            ClassicAssert.AreEqual(StepValidateResponse.Types.ErrorType.DuplicateStepImplementation,
                response.ErrorType);
            ClassicAssert.AreEqual("Multiple step implementations found for : step_text_1",
                response.ErrorMessage);
            ClassicAssert.IsEmpty(response.Suggestion);
        }

        [Test]
        public void ShouldGetErrorResponseForStepValidateRequestWhenNoImplFound()
        {
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0,
                StepValue = new ProtoStepValue
                {
                    ParameterizedStepValue = "step_text_1",
                    StepValue = "step_text_1"
                }
            };
            var processor = new StepValidationProcessor(_mockStepRegistry.Object);
            var response = processor.Process(request);

            ClassicAssert.AreEqual(false, response.IsValid);
            ClassicAssert.AreEqual(StepValidateResponse.Types.ErrorType.StepImplementationNotFound,
                response.ErrorType);
            StringAssert.Contains("No implementation found for : step_text_1.",
                response.ErrorMessage);
            StringAssert.Contains("[Step(\"step_text_1\")]", response.Suggestion);
        }


        [Test]
        public void ShouldGetValidResponseForStepValidateRequest()
        {
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0
            };

            _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
            _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(false);
            _mockStepRegistry.Setup(registry => registry.HasAsyncVoidImplementation("step_text_1")).Returns(false);

            var response = new StepValidationProcessor(_mockStepRegistry.Object).Process(request);

            Assert.That(response.IsValid, Is.True, $"Expected true, got error: {response.ErrorMessage}");
        }
        [Test]
        public void ShouldGetErrorResponseForStepValidateRequestWhenAsyncVoidImplFound()
        {
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0,
                StepValue = new ProtoStepValue
                {
                    ParameterizedStepValue = "step_text_1",
                    StepValue = "step_text_1"
                }
            };
            _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
            _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(false);
            _mockStepRegistry.Setup(registry => registry.HasAsyncVoidImplementation("step_text_1")).Returns(true);

            var processor = new StepValidationProcessor(_mockStepRegistry.Object);
            var response = processor.Process(request);

            ClassicAssert.AreEqual(false, response.IsValid);
            ClassicAssert.AreEqual(StepValidateResponse.Types.ErrorType.StepImplementationNotFound,
                response.ErrorType);
            StringAssert.StartsWith("Found a potential step implementation with 'async void' return for : step_text_1.",
                response.Suggestion);
            StringAssert.Contains("Usage of 'async void' is discouraged", response.Suggestion);
        }
    }
}