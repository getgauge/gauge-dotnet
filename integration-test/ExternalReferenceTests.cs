/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.DataStore;
using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Gauge.Dotnet.IntegrationTests;

public class ExternalReferenceTests
{
    protected readonly ILoggerFactory _loggerFactory = new LoggerFactory();

    [Test]
    [TestCase("DllReference", "Dll Reference: Vowels in English language are {}.", "Dll Reference: Vowels in English language are <vowelString>.", "Dll Reference: Vowels in English language are \"aeiou\".")]
    [TestCase("ProjectReference", "Project Reference: Vowels in English language are {}.", "Project Reference: Vowels in English language are <vowelString>.", "Project Reference: Vowels in English language are \"aeiou\".")]
    public async Task ShouldGetStepsFromReference(string referenceType, string stepText, string stepValue, string parameterizedStepValue)
    {
        var testProjectPath = TestUtils.GetIntegrationTestSampleDirectory(referenceType);
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(new Dictionary<string, string> { { "GAUGE_PROJECT_ROOT", testProjectPath } });
        var config = builder.Build();
        var fileProvider = new PhysicalFileProvider(Path.Combine(testProjectPath, "gauge_bin"));

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var gaugeLoadContext = new GaugeLoadContext(() => { return AssemblyLocater.GetTestAssembly(fileProvider); },
            _loggerFactory.CreateLogger<GaugeLoadContext>());
        var assemblyLoader = new AssemblyLoader(gaugeLoadContext, new ReflectionWrapper(),
            new ActivatorWrapper(serviceProvider), new StepRegistry(), _loggerFactory.CreateLogger<AssemblyLoader>(),
            (logger) => { return AssemblyLocater.GetAssembliesReferencingGaugeLib(fileProvider, logger); });

        var stepValidationProcessor = new StepValidationProcessor(assemblyLoader.GetStepRegistry());
        var message = new StepValidateRequest
        {
            StepText = stepText,
            StepValue = new ProtoStepValue { StepValue = stepValue, ParameterizedStepValue = parameterizedStepValue },
            NumberOfParameters = 1,
        };
        var result = await stepValidationProcessor.Process(1, message);

        ClassicAssert.IsTrue(result.IsValid, $"Expected valid step text, got error: {result.ErrorMessage}");
    }


    [Test]
    [TestCase("ProjectReference", "Take Screenshot in reference Project", "ReferenceProject-IDoNotExist.png")]
    [TestCase("DllReference", "Take Screenshot in reference DLL", "ReferenceDll-IDoNotExist.png")]
    public async Task ShouldRegisterScreenshotWriterFromReference(string referenceType, string stepText, string expected)
    {
        var testProjectPath = TestUtils.GetIntegrationTestSampleDirectory(referenceType);
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(new Dictionary<string, string> { { "GAUGE_PROJECT_ROOT", testProjectPath } });
        var config = builder.Build();
        var fileProvider = new PhysicalFileProvider(Path.Combine(testProjectPath, "gauge_bin"));

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper(serviceProvider);
        var gaugeLoadContext = new GaugeLoadContext(() => { return AssemblyLocater.GetTestAssembly(fileProvider); },
            _loggerFactory.CreateLogger<GaugeLoadContext>());
        var assemblyLoader = new AssemblyLoader(gaugeLoadContext, reflectionWrapper,
            activatorWrapper, new StepRegistry(), _loggerFactory.CreateLogger<AssemblyLoader>(),
            (logger) => { return AssemblyLocater.GetAssembliesReferencingGaugeLib(fileProvider, logger); });
        var hookRegistry = new HookRegistry(assemblyLoader);
        var dataStoreFactory = new DataStoreFactory(assemblyLoader, activatorWrapper);
        var tableFormatter = new TableFormatter(assemblyLoader, activatorWrapper);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper, dataStoreFactory, tableFormatter);
        var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry, _loggerFactory.CreateLogger<HookExecutor>()),
            new StepExecutor(assemblyLoader, _loggerFactory.CreateLogger<StepExecutor>(), executionInfoMapper), config,
            _loggerFactory.CreateLogger<ExecutionOrchestrator>(), dataStoreFactory);

        var executeStepProcessor = new ExecuteStepProcessor(assemblyLoader.GetStepRegistry(),
            executionOrchestrator, new TableFormatter(assemblyLoader, activatorWrapper));

        var message = new ExecuteStepRequest
        {
            ParsedStepText = stepText,
            ActualStepText = stepText
        };

        var result = await executeStepProcessor.Process(1, message);
        var protoExecutionResult = result.ExecutionResult;

        ClassicAssert.IsNotNull(protoExecutionResult);
        Console.WriteLine(protoExecutionResult.ScreenshotFiles[0]);
        ClassicAssert.AreEqual(protoExecutionResult.ScreenshotFiles[0], expected);
    }
}