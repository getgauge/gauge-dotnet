/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet.IntegrationTests;

public class ExternalReferenceTests
{
    [Test]
    [TestCase("DllReference", "Dll Reference: Vowels in English language are {}.", "Dll Reference: Vowels in English language are <vowelString>.", "Dll Reference: Vowels in English language are \"aeiou\".")]
    [TestCase("ProjectReference", "Project Reference: Vowels in English language are {}.", "Project Reference: Vowels in English language are <vowelString>.", "Project Reference: Vowels in English language are \"aeiou\".")]
    public async Task ShouldGetStepsFromReference(string referenceType, string stepText, string stepValue, string parameterizedStepValue)
    {
        Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TestUtils.GetIntegrationTestSampleDirectory(referenceType));
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), new ReflectionWrapper(), new ActivatorWrapper(), new StepRegistry());

        var stepValidationProcessor = new StepValidationProcessor(assemblyLoader.GetStepRegistry());
        var message = new StepValidateRequest
        {
            StepText = stepText,
            StepValue = new ProtoStepValue { StepValue = stepValue, ParameterizedStepValue = parameterizedStepValue },
            NumberOfParameters = 1,
        };
        var result = await stepValidationProcessor.Process(message);

        ClassicAssert.IsTrue(result.IsValid, $"Expected valid step text, got error: {result.ErrorMessage}");
    }


    [Test]
    [TestCase("ProjectReference", "Take Screenshot in reference Project", "ReferenceProject-IDoNotExist.png")]
    [TestCase("DllReference", "Take Screenshot in reference DLL", "ReferenceDll-IDoNotExist.png")]
    public void ShouldRegisterScreenshotWriterFromReference(string referenceType, string stepText, string expected)
    {
        Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TestUtils.GetIntegrationTestSampleDirectory(referenceType));
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper();
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), reflectionWrapper, activatorWrapper, new StepRegistry());
        var classInstanceManager = new ThreadLocal<object>(() => assemblyLoader.GetClassInstanceManager()).Value;
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
        var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            classInstanceManager,
            new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
            new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

        var executeStepProcessor = new ExecuteStepProcessor(assemblyLoader.GetStepRegistry(),
            executionOrchestrator, new TableFormatter(assemblyLoader, activatorWrapper));

        var message = new ExecuteStepRequest
        {
            ParsedStepText = stepText,
            ActualStepText = stepText
        };

        var result = executeStepProcessor.Process(message);
        var protoExecutionResult = result.ExecutionResult;

        ClassicAssert.IsNotNull(protoExecutionResult);
        Console.WriteLine(protoExecutionResult.ScreenshotFiles[0]);
        ClassicAssert.AreEqual(protoExecutionResult.ScreenshotFiles[0], expected);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
    }
}