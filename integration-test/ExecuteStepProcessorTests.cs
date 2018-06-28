// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Text;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using NUnit.Framework;

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
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager(activatorWrapper);
            var mockOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var executeStepProcessor = new ExecuteStepProcessor(assemblyLoader.GetStepRegistry(),
                mockOrchestrator, new TableFormatter(assemblyLoader, activatorWrapper));

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
            var message = new Message
            {
                MessageId = 1234,
                MessageType = Message.Types.MessageType.ExecuteStep,
                ExecuteStepRequest = new ExecuteStepRequest
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
                }
            };
            var result = executeStepProcessor.Process(message);

            AssertRunnerDomainDidNotLoadUsersAssembly();
            var protoExecutionResult = result.ExecutionStatusResponse.ExecutionResult;
            Assert.IsNotNull(protoExecutionResult);
            Assert.IsFalse(protoExecutionResult.Failed);
        }

        [Test]
        public void ShouldCaptureScreenshotOnFailure()
        {
            const string parameterizedStepText = "I throw a serializable exception";
            const string stepText = "I throw a serializable exception";
            var reflectionWrapper = new ReflectionWrapper();
            var activatorWrapper = new ActivatorWrapper();
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(), reflectionWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager(activatorWrapper);

            var mockOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            var executeStepProcessor = new ExecuteStepProcessor(assemblyLoader.GetStepRegistry(),
                mockOrchestrator, new TableFormatter(assemblyLoader, activatorWrapper));


            var message = new Message
            {
                MessageType = Message.Types.MessageType.ExecuteStep,
                ExecuteStepRequest = new ExecuteStepRequest
                {
                    ParsedStepText = parameterizedStepText,
                    ActualStepText = stepText
                }
            };

            var result = executeStepProcessor.Process(message);
            var protoExecutionResult = result.ExecutionStatusResponse.ExecutionResult;

            Assert.IsNotNull(protoExecutionResult);
            Assert.IsTrue(protoExecutionResult.Failed);
            Assert.AreEqual(Encoding.UTF8.GetString(protoExecutionResult.ScreenShot.ToByteArray()), "ScreenShot");
        }
    }
}