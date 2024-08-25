/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;

namespace Gauge.Dotnet.UnitTests;

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
    public async Task ShouldGetErrorResponseForStepValidateRequestWhenMultipleStepImplFound()
    {
        var request = new StepValidateRequest
        {
            StepText = "step_text_1",
            NumberOfParameters = 0
        };

        _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
        _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(true);
        var processor = new StepValidationProcessor(_mockStepRegistry.Object);
        var response = await processor.Process(request);

        ClassicAssert.AreEqual(false, response.IsValid);
        ClassicAssert.AreEqual(StepValidateResponse.Types.ErrorType.DuplicateStepImplementation,
            response.ErrorType);
        ClassicAssert.AreEqual("Multiple step implementations found for : step_text_1",
            response.ErrorMessage);
        ClassicAssert.IsEmpty(response.Suggestion);
    }

    [Test]
    public async Task ShouldGetErrorResponseForStepValidateRequestWhennNoImplFound()
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
        var response = await processor.Process(request);

        ClassicAssert.AreEqual(false, response.IsValid);
        ClassicAssert.AreEqual(StepValidateResponse.Types.ErrorType.StepImplementationNotFound,
            response.ErrorType);
        StringAssert.Contains("No implementation found for : step_text_1.",
            response.ErrorMessage);
        StringAssert.Contains("[Step(\"step_text_1\")]", response.Suggestion);
    }


    [Test]
    public async Task ShouldGetVaildResponseForStepValidateRequest()
    {
        var request = new StepValidateRequest
        {
            StepText = "step_text_1",
            NumberOfParameters = 0
        };

        _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
        _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(false);

        var processor = new StepValidationProcessor(_mockStepRegistry.Object);
        var response = await processor.Process(request);

        ClassicAssert.AreEqual(true, response.IsValid);
    }
}