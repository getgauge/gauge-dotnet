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
using System.Text;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Processors
{
    [TestFixture]
    public class ExecutionEndingProcessorTests
    {
        [SetUp]
        public void Setup()
        {
            var mockHookRegistry = new Mock<IHookRegistry>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockMethod = new MockMethodBuilder(mockAssemblyLoader)
                .WithName("Foo")
                .WithFilteredHook(LibType.BeforeSpec)
                .Build();
            var hooks = new HashSet<IHookMethod>
            {
                new HookMethod(LibType.BeforeSpec, mockMethod, mockAssemblyLoader.Object)
            };
            mockHookRegistry.Setup(x => x.AfterSuiteHooks).Returns(hooks);
            var executionEndingRequest = new ExecutionEndingRequest
            {
                CurrentExecutionInfo = new ExecutionInfo
                {
                    CurrentSpec = new SpecInfo(),
                    CurrentScenario = new ScenarioInfo()
                }
            };
            _request = executionEndingRequest;

            _mockMethodExecutor = new Mock<IExecutionOrchestrator>();
            _protoExecutionResult = new ProtoExecutionResult
            {
                ExecutionTime = 0,
                Failed = false
            };
            _mockMethodExecutor.Setup(x =>
                    x.ExecuteHooks("AfterSuite", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(),
                        It.IsAny<ExecutionContext>()))
                .Returns(_protoExecutionResult);
            _mockMethodExecutor.Setup(x =>
                x.GetAllPendingMessages()).Returns(_pendingMessages);
            _mockMethodExecutor.Setup(x =>
                x.GetAllPendingScreenshots()).Returns(_pendingScreenshots);
            _executionEndingProcessor = new ExecutionEndingProcessor(_mockMethodExecutor.Object);
        }

        private ExecutionEndingProcessor _executionEndingProcessor;
        private ExecutionEndingRequest _request;
        private Mock<IExecutionOrchestrator> _mockMethodExecutor;
        private ProtoExecutionResult _protoExecutionResult;
        private readonly IEnumerable<string> _pendingMessages = new List<string> {"Foo", "Bar"};

        private readonly IEnumerable<byte[]> _pendingScreenshots =
            new List<byte[]> {Encoding.ASCII.GetBytes("screenshot")};

        [Test]
        public void ShouldExtendFromHooksExecutionProcessor()
        {
            AssertEx.InheritsFrom<HookExecutionProcessor, ExecutionEndingProcessor>();
            AssertEx.DoesNotInheritsFrom<TaggedHooksFirstExecutionProcessor, ExecutionEndingProcessor>();
            AssertEx.DoesNotInheritsFrom<UntaggedHooksFirstExecutionProcessor, ExecutionEndingProcessor>();
        }

        [Test]
        public void ShouldGetEmptyTagListByDefault()
        {
            var tags = AssertEx.ExecuteProtectedMethod<ExecutionEndingProcessor>("GetApplicableTags", _request.CurrentExecutionInfo);
            Assert.IsEmpty(tags);
        }

        [Test]
        public void ShouldProcessHooks()
        {
            var result = _executionEndingProcessor.Process(_request);
            _mockMethodExecutor.VerifyAll();
            Assert.AreEqual(result.ExecutionResult.Message, _pendingMessages);
            Assert.AreEqual(result.ExecutionResult.Screenshots, _pendingScreenshots);
        }


    }
}