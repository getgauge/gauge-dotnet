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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gauge.Dotnet.IntegrationTests;

public class ExecuteStepProcessorTests : IntegrationTestsBase
{
    [Test]
    public async Task ShouldExecuteMethodFromRequest()
    {
        const string parameterizedStepText = "Step that takes a table {}";
        const string stepText = "Step that takes a table <table>";
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper(serviceProvider);
        var gaugeLoadContext = new GaugeLoadContext(() => { return AssemblyLocater.GetTestAssembly(_fileProvider); }, _loggerFactory.CreateLogger<GaugeLoadContext>());
        var assemblyLoader = new AssemblyLoader(gaugeLoadContext, reflectionWrapper, activatorWrapper, new StepRegistry(),
            _loggerFactory.CreateLogger<AssemblyLoader>(), (logger) => { return AssemblyLocater.GetAssembliesReferencingGaugeLib(_fileProvider, logger); });
        var dataStoreFactory = new DataStoreFactory(assemblyLoader, activatorWrapper);
        var tableFormatter = new TableFormatter(assemblyLoader, activatorWrapper);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper, dataStoreFactory, tableFormatter);
        var hookRegistry = new HookRegistry(assemblyLoader);
        var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry, _loggerFactory.CreateLogger<HookExecutor>()),
            new StepExecutor(assemblyLoader, _loggerFactory.CreateLogger<StepExecutor>(), executionInfoMapper), _configuration, _loggerFactory.CreateLogger<ExecutionOrchestrator>(),
            dataStoreFactory);

        var executeStepProcessor = new ExecuteStepProcessor(assemblyLoader.GetStepRegistry(),
            orchestrator, new TableFormatter(assemblyLoader, activatorWrapper));

        var protoTable = new ProtoTable
        {
            Headers = new ProtoTableRow
            {
                Cells = { "foo", "bar" }
            },
            Rows =
            {
                new ProtoTableRow
                {
                    Cells = {"foorow1", "foorow2"}
                }
            }
        };
        var message = new ExecuteStepRequest
        {
            ParsedStepText = parameterizedStepText,
            ActualStepText = stepText,
            Parameters =
                {
                    new Parameter
                    {
                        Name = "table",
                        ParameterType = Parameter.Types.ParameterType.Table,
                        Table = protoTable
                    }
                }
        };
        var result = await executeStepProcessor.Process(1, message);

        var protoExecutionResult = result.ExecutionResult;
        ClassicAssert.IsNotNull(protoExecutionResult);
        ClassicAssert.IsFalse(protoExecutionResult.Failed);
    }

    [Test]
    public async Task ShouldCaptureScreenshotOnFailure()
    {
        const string stepText = "I throw a serializable exception";
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper(serviceProvider);
        var gaugeLoadContext = new GaugeLoadContext(() => { return AssemblyLocater.GetTestAssembly(_fileProvider); },
            _loggerFactory.CreateLogger<GaugeLoadContext>());
        var assemblyLoader = new AssemblyLoader(gaugeLoadContext, reflectionWrapper,
            activatorWrapper, new StepRegistry(), _loggerFactory.CreateLogger<AssemblyLoader>(),
            (logger) => { return AssemblyLocater.GetAssembliesReferencingGaugeLib(_fileProvider, logger); });
        var hookRegistry = new HookRegistry(assemblyLoader);
        var dataStoreFactory = new DataStoreFactory(assemblyLoader, activatorWrapper);
        var tableFormatter = new TableFormatter(assemblyLoader, activatorWrapper);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper, dataStoreFactory, tableFormatter);
        var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry, _loggerFactory.CreateLogger<HookExecutor>()),
            new StepExecutor(assemblyLoader, _loggerFactory.CreateLogger<StepExecutor>(), executionInfoMapper), _configuration,
            _loggerFactory.CreateLogger<ExecutionOrchestrator>(), dataStoreFactory);

        var executeStepProcessor = new ExecuteStepProcessor(assemblyLoader.GetStepRegistry(),
            orchestrator, new TableFormatter(assemblyLoader, activatorWrapper));


        var message = new ExecuteStepRequest
        {
            ParsedStepText = stepText,
            ActualStepText = stepText
        };

        var result = await executeStepProcessor.Process(1, message);
        var protoExecutionResult = result.ExecutionResult;

        ClassicAssert.IsNotNull(protoExecutionResult);
        ClassicAssert.IsTrue(protoExecutionResult.Failed);
        ClassicAssert.AreEqual("screenshot.png", protoExecutionResult.FailureScreenshotFile);
    }
}