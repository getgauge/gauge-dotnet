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

using System.Collections.Generic;
using System.Linq;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Wrappers;
using NUnit.Framework;

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
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper, activatorWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));
            var gaugeMethod = assemblyLoader.GetStepRegistry()
                .MethodFor("I throw a serializable exception and continue");
            var executionResult = orchestrator.ExecuteStep(gaugeMethod);
            Assert.IsTrue(executionResult.Failed);
            Assert.IsTrue(executionResult.RecoverableError);
        }

        [Test]
        public void ShouldCreateTableFromTargetType()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper, activatorWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));
            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("Step that takes a table {}");
            var table = new Table(new List<string> {"foo", "bar"});
            table.AddRow(new List<string> {"foorow1", "barrow1"});
            table.AddRow(new List<string> {"foorow2", "barrow2"});

            var executionResult = orchestrator.ExecuteStep(gaugeMethod, SerializeTable(table));
            Assert.False(executionResult.Failed);
        }

        [Test]
        public void ShouldExecuteMethodAndReturnResult()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper, activatorWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));
            AssertRunnerDomainDidNotLoadUsersAssembly();
            var gaugeMethod = assemblyLoader.GetStepRegistry()
                .MethodFor("A context step which gets executed before every scenario");

            var executionResult = orchestrator.ExecuteStep(gaugeMethod);
            Assert.False(executionResult.Failed);
        }


        [Test]
        public void ShouldGetPendingMessages()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper, activatorWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("Say {} to {}");

            var executionResult = executionOrchestrator.ExecuteStep(gaugeMethod, "hello", "world");

            Assert.False(executionResult.Failed);
            Assert.Contains("hello, world!", executionResult.Message);
        }

        [Test]
        public void ShouldGetStacktraceForAggregateException()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper, activatorWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("I throw an AggregateException");
            var executionResult = executionOrchestrator.ExecuteStep(gaugeMethod);

            Assert.True(executionResult.Failed);
            Assert.True(executionResult.StackTrace.Contains("First Exception"));
            Assert.True(executionResult.StackTrace.Contains("Second Exception"));
        }

        [Test]
        public void ShouldGetStepTextsForMethod()
        {
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper, activatorWrapper);
            var registry = assemblyLoader.GetStepRegistry();
            var gaugeMethod = registry.MethodFor("and an alias");
            var stepTexts = gaugeMethod.Aliases.ToList();

            Assert.Contains("Step with text", stepTexts);
            Assert.Contains("and an alias", stepTexts);
        }

        [Test]
        public void SuccessIsFalseOnSerializableExceptionThrown()
        {
            const string expectedMessage = "I am a custom serializable exception";
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper, activatorWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));
            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("I throw a serializable exception");

            var executionResult = executionOrchestrator.ExecuteStep(gaugeMethod);

            Assert.True(executionResult.Failed);
            Assert.AreEqual(expectedMessage, executionResult.ErrorMessage);
            StringAssert.Contains("IntegrationTestSample.StepImplementation.ThrowSerializableException",
                executionResult.StackTrace);
        }

        [Test]
        public void SuccessIsFalseOnUnserializableExceptionThrown()
        {
            const string expectedMessage = "I am a custom exception";
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper, activatorWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            AssertRunnerDomainDidNotLoadUsersAssembly();
            var gaugeMethod = assemblyLoader.GetStepRegistry().MethodFor("I throw an unserializable exception");
            var executionResult = executionOrchestrator.ExecuteStep(gaugeMethod);
            Assert.True(executionResult.Failed);
            Assert.AreEqual(expectedMessage, executionResult.ErrorMessage);
            StringAssert.Contains("IntegrationTestSample.StepImplementation.ThrowUnserializableException",
                executionResult.StackTrace);
        }
    }
}