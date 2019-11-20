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

            Assert.AreEqual(false, response.IsValid);
            Assert.AreEqual(StepValidateResponse.Types.ErrorType.DuplicateStepImplementation,
                response.ErrorType);
            Assert.AreEqual("Multiple step implementations found for : step_text_1",
                response.ErrorMessage);
            Assert.IsEmpty(response.Suggestion);
        }

        [Test]
        public void ShouldGetErrorResponseForStepValidateRequestWhennNoImplFound()
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

            Assert.AreEqual(false, response.IsValid);
            Assert.AreEqual(StepValidateResponse.Types.ErrorType.StepImplementationNotFound,
                response.ErrorType);
            StringAssert.Contains("No implementation found for : step_text_1.",
                response.ErrorMessage);
            StringAssert.Contains("[Step(\"step_text_1\")]", response.Suggestion);
        }


        [Test]
        public void ShouldGetVaildResponseForStepValidateRequest()
        {
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0
            };

            _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
            _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(false);

            var processor = new StepValidationProcessor(_mockStepRegistry.Object);
            var response = processor.Process(request);

            Assert.AreEqual(true, response.IsValid);
        }
    }
}