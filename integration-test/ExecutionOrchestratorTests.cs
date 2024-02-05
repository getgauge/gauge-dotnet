/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Linq;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.IntegrationTests
{
    [TestFixture]
    public class ExecutionOrchestratorTests : IntegrationTestsBase
    {
        [Test]
        public void RecoverableIsTrueOnExceptionThrownWhenContinueOnFailure()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));
            var gaugeMethod = assemblyLoader.GetStepRegistry()
                .MethodFor("I throw a serializable exception and continue");
            var executionResult = orchestrator.ExecuteStep(gaugeMethod);
            ClassicAssert.IsTrue(executionResult.Failed);
            ClassicAssert.IsTrue(executionResult.RecoverableError);
        }

        [Test]
        public void ShouldCreateTableFromTargetType()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));
            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("Step that takes a table {}");
            var table = new Table(new List<string> { "foo", "bar" });
            table.AddRow(new List<string> { "foorow1", "barrow1" });
            table.AddRow(new List<string> { "foorow2", "barrow2" });

            var executionResult = orchestrator.ExecuteStep(gaugeMethod, SerializeTable(table));
            ClassicAssert.False(executionResult.Failed);
        }

        [Test]
        public void ShouldExecuteMethodAndReturnResult()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));
            var gaugeMethod = assemblyLoader.GetStepRegistry()
                .MethodFor("A context step which gets executed before every scenario");

            var executionResult = orchestrator.ExecuteStep(gaugeMethod);
            ClassicAssert.False(executionResult.Failed);
        }

        [Test]
        public void ShouldGetPendingMessages()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("Say {} to {}");

            var executionResult = executionOrchestrator.ExecuteStep(gaugeMethod, "hello", "world");

            ClassicAssert.False(executionResult.Failed);
            ClassicAssert.Contains("hello, world!", executionResult.Message);
        }

        [Test]
        public void ShouldExecuteAsyncStepImplementation()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("Say {} to {} async");

            var executionResult = executionOrchestrator.ExecuteStep(gaugeMethod, "hello", "async world");

            Assert.That(executionResult.Failed, Is.False, executionResult.ErrorMessage);
            StringAssert.Contains("hello, async world!", executionResult.Message.ToString());
        }
        
        [Test]
        public void ShouldGetStacktraceForAggregateException()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("I throw an AggregateException");
            var executionResult = executionOrchestrator.ExecuteStep(gaugeMethod);

            ClassicAssert.True(executionResult.Failed);
            ClassicAssert.True(executionResult.StackTrace.Contains("First Exception"));
            ClassicAssert.True(executionResult.StackTrace.Contains("Second Exception"));
        }

        [Test]
        public void ShouldGetStepTextsForMethod()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var registry = assemblyLoader.GetStepRegistry();
            var gaugeMethod = registry.MethodFor("and an alias");
            var stepTexts = gaugeMethod.Aliases.ToList();

            ClassicAssert.Contains("Step with text", stepTexts);
            ClassicAssert.Contains("and an alias", stepTexts);
        }

        [Test]
        public void SuccessIsFalseOnSerializableExceptionThrown()
        {
            const string expectedMessage = "I am a custom serializable exception";
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));
            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("I throw a serializable exception");

            var executionResult = executionOrchestrator.ExecuteStep(gaugeMethod);

            ClassicAssert.True(executionResult.Failed);
            ClassicAssert.AreEqual(expectedMessage, executionResult.ErrorMessage);
            StringAssert.Contains("IntegrationTestSample.StepImplementation.ThrowSerializableException",
                executionResult.StackTrace);
        }

        [Test]
        public void SuccessIsFalseOnUnserializableExceptionThrown()
        {
            const string expectedMessage = "I am a custom exception";
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("I throw an unserializable exception");
            var executionResult = executionOrchestrator.ExecuteStep(gaugeMethod);
            ClassicAssert.True(executionResult.Failed);
            ClassicAssert.AreEqual(expectedMessage, executionResult.ErrorMessage);
            StringAssert.Contains("IntegrationTestSample.StepImplementation.ThrowUnserializableException",
                executionResult.StackTrace);
        }
    }
}