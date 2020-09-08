/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using NUnit.Framework;
using System.Linq;

namespace Gauge.Dotnet.IntegrationTests
{
    public class ExecuteStepProcessorTests : IntegrationTestsBase
    {
        [Test]
        public void ShouldExecuteMethodFromRequest()
        {
            const string parameterizedStepText = "Step that takes a table {}";
            const string stepText = "Step that takes a table <table>";
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var executeStepProcessor = new ExecuteStepProcessor(assemblyLoader.GetStepRegistry(),
                orchestrator, new TableFormatter(assemblyLoader, activatorWrapper));

            var protoTable = new ProtoTable
            {
                Headers = new ProtoTableRow
                {
                    Cells = {"foo", "bar"}
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
            var result = executeStepProcessor.Process(message);

            var protoExecutionResult = result.ExecutionResult;
            Assert.IsNotNull(protoExecutionResult);
            Assert.IsFalse(protoExecutionResult.Failed);
        }

        [Test]
        public void ShouldCaptureScreenshotOnFailure()
        {
            const string stepText = "I throw a serializable exception";
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var path = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            var assemblyLoader = new AssemblyLoader(path, new GaugeLoadContext(path), reflectionWrapper, activatorWrapper, new StepRegistry());
            var classInstanceManager = assemblyLoader.GetClassInstanceManager();
            var executionInfoMapper = new ExecutionInfoMapper(assemblyLoader, activatorWrapper);
            var orchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager, executionInfoMapper),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var executeStepProcessor = new ExecuteStepProcessor(assemblyLoader.GetStepRegistry(),
                orchestrator, new TableFormatter(assemblyLoader, activatorWrapper));


            var message = new ExecuteStepRequest
            {
                ParsedStepText = stepText,
                ActualStepText = stepText
            };

            var result = executeStepProcessor.Process(message);
            var protoExecutionResult = result.ExecutionResult;

            Assert.IsNotNull(protoExecutionResult);
            Assert.IsTrue(protoExecutionResult.Failed);
            Assert.AreEqual("screenshot.png", protoExecutionResult.FailureScreenshotFile);
        }
    }
}