/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.CSharp.Lib;
using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;

namespace Gauge.Dotnet.IntegrationTests;

[TestFixture]
public class ExecutionOrchestratorTests : IntegrationTestsBase
{
    [Test]
    public async Task RecoverableIsTrueOnExceptionThrownWhenContinueOnFailure()
    {
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper();
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), reflectionWrapper, activatorWrapper, new StepRegistry());
        var hookRegistry = new HookRegistry(assemblyLoader);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
        var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry),
            new StepExecutor(assemblyLoader));
        var gaugeMethod = assemblyLoader.GetStepRegistry()
            .MethodFor("I throw a serializable exception and continue");
        var executionResult = await orchestrator.ExecuteStep(gaugeMethod, 1);
        ClassicAssert.IsTrue(executionResult.Failed);
        ClassicAssert.IsTrue(executionResult.RecoverableError);
    }

    [Test]
    public async Task ShouldCreateTableFromTargetType()
    {
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper();
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), reflectionWrapper, activatorWrapper, new StepRegistry());
        var hookRegistry = new HookRegistry(assemblyLoader);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
        var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry),
            new StepExecutor(assemblyLoader));
        var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("Step that takes a table {}");
        var table = new Table(new List<string> { "foo", "bar" });
        table.AddRow(new List<string> { "foorow1", "barrow1" });
        table.AddRow(new List<string> { "foorow2", "barrow2" });

        var executionResult = await orchestrator.ExecuteStep(gaugeMethod, 1, SerializeTable(table));
        ClassicAssert.False(executionResult.Failed);
    }

    [Test]
    public async Task ShouldExecuteMethodAndReturnResult()
    {
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper();
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), reflectionWrapper, activatorWrapper, new StepRegistry());
        var hookRegistry = new HookRegistry(assemblyLoader);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
        var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry),
            new StepExecutor(assemblyLoader));
        var gaugeMethod = assemblyLoader.GetStepRegistry()
            .MethodFor("A context step which gets executed before every scenario");

        var executionResult = await orchestrator.ExecuteStep(gaugeMethod, 1);
        ClassicAssert.False(executionResult.Failed);
    }


    [Test]
    public async Task ShouldGetPendingMessages()
    {
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper();
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), reflectionWrapper, activatorWrapper, new StepRegistry());
        var hookRegistry = new HookRegistry(assemblyLoader);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
        var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry),
            new StepExecutor(assemblyLoader));

        var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("Say {} to {}");

        var executionResult = await executionOrchestrator.ExecuteStep(gaugeMethod, 1, "hello", "world");

        ClassicAssert.False(executionResult.Failed);
        ClassicAssert.Contains("hello, world!", executionResult.Message);
    }

    [Test]
    public async Task ShouldGetStacktraceForAggregateException()
    {
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper();
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), reflectionWrapper, activatorWrapper, new StepRegistry());
        var hookRegistry = new HookRegistry(assemblyLoader);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
        var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry),
            new StepExecutor(assemblyLoader));

        var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("I throw an AggregateException");
        var executionResult = await executionOrchestrator.ExecuteStep(gaugeMethod, 1);

        ClassicAssert.True(executionResult.Failed);
        ClassicAssert.True(executionResult.StackTrace.Contains("First Exception"));
        ClassicAssert.True(executionResult.StackTrace.Contains("Second Exception"));
    }

    [Test]
    public void ShouldGetStepTextsForMethod()
    {
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper();
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), reflectionWrapper, activatorWrapper, new StepRegistry());
        var registry = assemblyLoader.GetStepRegistry();
        var gaugeMethod = registry.MethodFor("and an alias");
        var stepTexts = gaugeMethod.Aliases.ToList();

        ClassicAssert.Contains("Step with text", stepTexts);
        ClassicAssert.Contains("and an alias", stepTexts);
    }

    [Test]
    public async Task SuccessIsFalseOnSerializableExceptionThrown()
    {
        const string expectedMessage = "I am a custom serializable exception";
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper();
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), reflectionWrapper, activatorWrapper, new StepRegistry());
        var hookRegistry = new HookRegistry(assemblyLoader);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
        var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry),
            new StepExecutor(assemblyLoader));
        var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("I throw a serializable exception");

        var executionResult = await executionOrchestrator.ExecuteStep(gaugeMethod, 1);

        ClassicAssert.True(executionResult.Failed);
        ClassicAssert.AreEqual(expectedMessage, executionResult.ErrorMessage);
        StringAssert.Contains("IntegrationTestSample.StepImplementation.ThrowSerializableException",
            executionResult.StackTrace);
    }

    [Test]
    public async Task SuccessIsFalseOnUnserializableExceptionThrown()
    {
        const string expectedMessage = "I am a custom exception";
        var reflectionWrapper = new ReflectionWrapper();
        var activatorWrapper = new ActivatorWrapper();
        var assemblyLocator = new AssemblyLocater(new DirectoryWrapper());
        var assemblyLoader = new AssemblyLoader(assemblyLocator, new GaugeLoadContext(assemblyLocator), reflectionWrapper, activatorWrapper, new StepRegistry());
        var hookRegistry = new HookRegistry(assemblyLoader);
        var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
        var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
            new HookExecutor(assemblyLoader, executionInfoMapper, hookRegistry),
            new StepExecutor(assemblyLoader));

        var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("I throw an unserializable exception");
        var executionResult = await executionOrchestrator.ExecuteStep(gaugeMethod, 1);
        ClassicAssert.True(executionResult.Failed);
        ClassicAssert.AreEqual(expectedMessage, executionResult.ErrorMessage);
        StringAssert.Contains("IntegrationTestSample.StepImplementation.ThrowUnserializableException",
            executionResult.StackTrace);
    }
}