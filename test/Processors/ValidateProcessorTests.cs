/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Registries;
using Gauge.Messages;
using Microsoft.Extensions.Logging;


namespace Gauge.Dotnet.UnitTests.Processors;

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

        _mockStepRegistry.Setup(registry => registry.LookupStep("step_text_1")).Returns(
            new StepLookupResult(true, true, new[]
            {
                new GaugeMethod { Name = "StepImpl", ClassName = "StepsA", FileName = "StepsA.cs" },
                new GaugeMethod { Name = "StepImpl", ClassName = "StepsB", FileName = "StepsB.cs" }
            }));
        var processor = new StepValidationProcessor(_mockStepRegistry.Object, Mock.Of<ILogger<StepValidationProcessor>>());
        var response = await processor.Process(1, request);

        ClassicAssert.AreEqual(false, response.IsValid);
        ClassicAssert.AreEqual(StepValidateResponse.Types.ErrorType.DuplicateStepImplementation,
            response.ErrorType);
        StringAssert.Contains("Step: step_text_1", response.ErrorMessage);
        StringAssert.Contains("StepsA.StepImpl in StepsA.cs:1", response.ErrorMessage);
        StringAssert.Contains("StepsB.StepImpl in StepsB.cs:1", response.ErrorMessage);
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
        _mockStepRegistry.Setup(registry => registry.LookupStep("step_text_1")).Returns(
            new StepLookupResult(false, false, Array.Empty<GaugeMethod>()));
        var processor = new StepValidationProcessor(_mockStepRegistry.Object, Mock.Of<ILogger<StepValidationProcessor>>());
        var response = await processor.Process(1, request);

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

        _mockStepRegistry.Setup(registry => registry.LookupStep("step_text_1")).Returns(
            new StepLookupResult(true, false, new[] { new GaugeMethod { Name = "StepImpl" } }));

        var processor = new StepValidationProcessor(_mockStepRegistry.Object, Mock.Of<ILogger<StepValidationProcessor>>());
        var response = await processor.Process(1, request);

        ClassicAssert.AreEqual(true, response.IsValid);
    }
}