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

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using Moq;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    public class ExecuteStepProcessorTests
    {
#pragma warning disable RECS0154 // Parameter is never used
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Foo(string param)
#pragma warning restore xUnit1013 // Public method should be marked as test
#pragma warning restore RECS0154 // Parameter is never used
        {
        }

        private static bool HasTable(IReadOnlyList<string> parameters)
        {
            var serializer = new DataContractJsonSerializer(typeof(Table));

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(parameters[0])))
            {
                try
                {
                    serializer.ReadObject(stream);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        [Fact]
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

            var response =
                new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object).Process(request);

            Assert.False(response.ExecutionStatusResponse.ExecutionResult.Failed);
        }

        [Theory]
        [InlineData(Parameter.Types.ParameterType.Table)]
        [InlineData(Parameter.Types.ParameterType.SpecialTable)]
        public void ShouldProcessExecuteStepRequestForTableParam(Parameter.Types.ParameterType parameterType)
        {
            const string parsedStepText = "Foo";
            var protoTable = new ProtoTable();
            var headers = new ProtoTableRow();
            headers.Cells.AddRange(new List<string> {"foo", "bar"});
            protoTable.Headers = headers;
            var row = new ProtoTableRow();
            row.Cells.AddRange(new List<string> {"foorow1", "foorow2"});
            protoTable.Rows.AddRange(new List<ProtoTableRow> {row});

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

            var response =
                new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object).Process(request);

            mockMethodExecutor.Verify(executor =>
                executor.Execute(fooMethodInfo, It.Is<string[]>(strings => HasTable(strings))));
            Assert.False(response.ExecutionStatusResponse.ExecutionResult.Failed);
        }

        [Fact]
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

            var response =
                new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object).Process(request);

            Assert.True(response.ExecutionStatusResponse.ExecutionResult.Failed);
            Assert.Equal("Argument length mismatch for Foo. Actual Count: 0, Expected Count: 1", 
                response.ExecutionStatusResponse.ExecutionResult.ErrorMessage);
        }

        [Fact]
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

            var response =
                new ExecuteStepProcessor(mockStepRegistry.Object, mockMethodExecutor.Object).Process(request);

            Assert.True(response.ExecutionStatusResponse.ExecutionResult.Failed);
            Assert.Equal("Step Implementation not found", 
                response.ExecutionStatusResponse.ExecutionResult.ErrorMessage);
        }
    }
}