// Copyright 2015 ThoughtWorks, Inc.
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

using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Processors
{
    [TestFixture]
    public class ExecuteStepProcessorTests
    {
        public void Foo(string param)
        {
        }

        [Test]
        public void ShouldProcessExecuteStepRequest()
        {
            const string parsedStepText = "Foo";
            var request = new Message
            {
                MessageType = Message.Types.MessageType.ExecuteStep,
                MessageId = 20,
                ExecuteStepRequest = new ExecuteStepRequest
                {
                    ActualStepText = parsedStepText,
                    ParsedStepText = parsedStepText,
                    Parameters =
                    {
                        new Parameter
                        {
                            ParameterType = Parameter.Types.ParameterType.Static,
                            Name = "Foo",
                            Value = "Bar"
                        }
                    }
                }
            };
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(true);
            var fooMethodInfo = new GaugeMethod {Name = "Foo", ParameterCount = 1};
            mockStepRegistry.Setup(x => x.MethodFor(parsedStepText)).Returns(fooMethodInfo);
            var mockMethodExecutor = new Mock<IMethodExecutor>();
            mockMethodExecutor.Setup(e => e.Execute(fooMethodInfo, It.IsAny<string[]>()))
                .Returns(() => new ProtoExecutionResult {ExecutionTime = 1, Failed = false});

            var mockTableFormatter = new Mock<ITableFormatter>();

            var response =
                new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object, mockTableFormatter.Object)
                    .Process(request);

            Assert.False(response.ExecutionStatusResponse.ExecutionResult.Failed);
        }

        [Test]
        [TestCase(Parameter.Types.ParameterType.Table)]
        [TestCase(Parameter.Types.ParameterType.SpecialTable)]
        public void ShouldProcessExecuteStepRequestForTableParam(Parameter.Types.ParameterType parameterType)
        {
            const string parsedStepText = "Foo";
            var protoTable = new ProtoTable();
            var tableJSON = "{'headers':['foo', 'bar'],'rows':[['foorow1','barrow1']]}";
            var request = new Message
            {
                MessageType = Message.Types.MessageType.ExecuteStep,
                ExecuteStepRequest = new ExecuteStepRequest
                {
                    ActualStepText = parsedStepText,
                    ParsedStepText = parsedStepText,
                    Parameters =
                    {
                        new Parameter
                        {
                            ParameterType = parameterType,
                            Table = protoTable
                        }
                    }
                },
                MessageId = 20
            };

            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(true);
            var fooMethodInfo = new GaugeMethod {Name = "Foo", ParameterCount = 1};
            mockStepRegistry.Setup(x => x.MethodFor(parsedStepText)).Returns(fooMethodInfo);
            var mockMethodExecutor = new Mock<IMethodExecutor>();
            mockMethodExecutor.Setup(e => e.Execute(fooMethodInfo, It.IsAny<string[]>())).Returns(() =>
                new ProtoExecutionResult
                {
                    ExecutionTime = 1,
                    Failed = false
                });

            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector));
            var mockTableFormatter = new Mock<ITableFormatter>();
            mockTableFormatter.Setup(x => x.GetJSON(protoTable))
                .Returns(tableJSON);
            var response =
                new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object, mockTableFormatter.Object)
                    .Process(request);

            mockMethodExecutor.Verify(executor =>
                executor.Execute(fooMethodInfo, It.Is<string[]>(strings => strings[0] == tableJSON)));
            Assert.False(response.ExecutionStatusResponse.ExecutionResult.Failed);
        }

        [Test]
        public void ShouldReportArgumentMismatch()
        {
            const string parsedStepText = "Foo";
            var request = new Message
            {
                MessageType = Message.Types.MessageType.ExecuteStep,
                MessageId = 20,
                ExecuteStepRequest = new ExecuteStepRequest
                {
                    ActualStepText = parsedStepText,
                    ParsedStepText = parsedStepText
                }
            };
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(true);
            var fooMethod = new GaugeMethod {Name = "Foo", ParameterCount = 1};
            mockStepRegistry.Setup(x => x.MethodFor(parsedStepText)).Returns(fooMethod);
            var mockMethodExecutor = new Mock<IMethodExecutor>();

            var mockTableFormatter = new Mock<ITableFormatter>();

            var response =
                new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object, mockTableFormatter.Object)
                    .Process(request);

            Assert.True(response.ExecutionStatusResponse.ExecutionResult.Failed);
            Assert.AreEqual(response.ExecutionStatusResponse.ExecutionResult.ErrorMessage,
                "Argument length mismatch for Foo. Actual Count: 0, Expected Count: 1");
        }

        [Test]
        public void ShouldReportMissingStep()
        {
            const string parsedStepText = "Foo";
            var request = new Message
            {
                MessageType = Message.Types.MessageType.ExecuteStep,
                ExecuteStepRequest = new ExecuteStepRequest
                {
                    ActualStepText = parsedStepText,
                    ParsedStepText = parsedStepText
                },
                MessageId = 20
            };
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.ContainsStep(parsedStepText)).Returns(false);
            var mockMethodExecutor = new Mock<IMethodExecutor>();
            var mockTableFormatter = new Mock<ITableFormatter>();

            var response =
                new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object, mockTableFormatter.Object)
                    .Process(request);

            Assert.True(response.ExecutionStatusResponse.ExecutionResult.Failed);
            Assert.AreEqual(response.ExecutionStatusResponse.ExecutionResult.ErrorMessage,
                "Step Implementation not found");
        }
    }
}