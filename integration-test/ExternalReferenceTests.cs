/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Threading;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.Dotnet.IntegrationTests
{
    public class ExternalReferenceTests
    {
        [Test]
        [TestCase("DllReference", "Dll Reference: Vowels in English language are {}.", "Dll Reference: Vowels in English language are <vowelString>.", "Dll Reference: Vowels in English language are \"aeiou\".")]
        [TestCase("ProjectReference", "Project Reference: Vowels in English language are {}.", "Project Reference: Vowels in English language are <vowelString>.", "Project Reference: Vowels in English language are \"aeiou\".")]
        public void ShouldGetStepsFromReference(string referenceType, string stepText, string stepValue, string parameterizedStepValue)
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TestUtils.GetIntegrationTestSampleDirectory(referenceType));
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), new ReflectionWrapper(), new ActivatorWrapper(), new StepRegistry());

            var stepValidationProcessor = new StepValidationProcessor(assemblyLoader.GetStepRegistry());
            var message = new StepValidateRequest
            {
                StepText = stepText,
                StepValue = new ProtoStepValue { StepValue = stepValue, ParameterizedStepValue = parameterizedStepValue },
                NumberOfParameters = 1,
            };
            var result = stepValidationProcessor.Process(message);

            Assert.IsTrue(result.IsValid, $"Expected valid step text, got error: {result.ErrorMessage}");
        }


        [Test]
        [TestCase("ProjectReference", "Take Screenshot in reference Project", "ReferenceProject-IDoNotExist.png")]
        [TestCase("DllReference", "Take Screenshot in reference DLL", "ReferenceDll-IDoNotExist.png")]
        public void ShouldRegisterScreenshotWriterFromReference(string referenceType, string stepText, string expected)
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TestUtils.GetIntegrationTestSampleDirectory(referenceType));
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
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

            Assert.IsNotNull(protoExecutionResult);
            Console.WriteLine(protoExecutionResult.ScreenshotFiles[0]);
            Assert.AreEqual(protoExecutionResult.ScreenshotFiles[0], expected);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }
    }
}