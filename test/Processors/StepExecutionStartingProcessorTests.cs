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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Processors
{
    internal class StepExecutionStartingProcessorTests
    {
        [Test]
        public void ShouldExtendFromHooksExecutionProcessor()
        {
            AssertEx.InheritsFrom<UntaggedHooksFirstExecutionProcessor, StepExecutionStartingProcessor>();
        }

        [Test]
        public void ShouldClearExistingGaugeMessages()
        {
            var methodExecutor = new Mock<IMethodExecutor>();

            var request = new Message
            {
                MessageId = 20,
                MessageType = Message.Types.MessageType.ScenarioExecutionStarting,
                StepExecutionStartingRequest = new StepExecutionStartingRequest
                {
                    CurrentExecutionInfo = new ExecutionInfo
                    {
                        CurrentSpec = new SpecInfo(),
                        CurrentScenario = new ScenarioInfo()
                    }
                }
            };

            var protoExecutionResult = new ProtoExecutionResult {ExecutionTime = 0, Failed = false};
            methodExecutor.Setup(executor =>
                    executor.ExecuteHooks(It.IsAny<string>(), It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(), It.IsAny<Gauge.CSharp.Lib.ExecutionContext>()))
                .Returns(protoExecutionResult);
            var hookRegistry = new Mock<IHookRegistry>();
            hookRegistry.Setup(registry => registry.BeforeStepHooks).Returns(new HashSet<IHookMethod>());
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockMessageCollectorType = new Mock<Type>();
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            mockReflectionWrapper.Setup(x => x.InvokeMethod(mockMessageCollectorType.Object, null, "Clear", BindingFlags.Static | BindingFlags.Public))
                .Verifiable();
            mockAssemblyLoader.Setup(x => x.GetLibType(LibType.MessageCollector))
                .Returns(mockMessageCollectorType.Object);

            var processor = new StepExecutionStartingProcessor(methodExecutor.Object, mockAssemblyLoader.Object, mockReflectionWrapper.Object);
            processor.Process(request);

            mockReflectionWrapper.VerifyAll();
        }

        [Test]
        public void ShouldGetTagListFromScenarioAndSpec()
        {
            var specInfo = new SpecInfo
            {
                Tags = {"foo"},
                Name = "",
                FileName = "",
                IsFailed = false
            };
            var scenarioInfo = new ScenarioInfo
            {
                Tags = {"bar"},
                Name = "",
                IsFailed = false
            };
            var currentScenario = new ExecutionInfo
            {
                CurrentScenario = scenarioInfo,
                CurrentSpec = specInfo
            };
            var currentExecutionInfo = new StepExecutionStartingRequest
            {
                CurrentExecutionInfo = currentScenario
            };

            var message = new Message
            {
                StepExecutionStartingRequest = currentExecutionInfo,
                MessageType = Message.Types.MessageType.ScenarioExecutionStarting,
                MessageId = 0
            };
            var tags = AssertEx.ExecuteProtectedMethod<StepExecutionStartingProcessor>("GetApplicableTags", message)
                .ToList();
            Assert.IsNotEmpty(tags);
            Assert.AreEqual(2, tags.Count);
            Assert.Contains("foo", tags);
            Assert.Contains("bar", tags);
        }

        [Test]
        public void ShouldGetTagListFromScenarioAndSpecAndIgnoreDuplicates()
        {
            var specInfo = new SpecInfo
            {
                Tags = {"foo"},
                Name = "",
                FileName = "",
                IsFailed = false
            };
            var scenarioInfo = new ScenarioInfo
            {
                Tags = {"foo"},
                Name = "",
                IsFailed = false
            };
            var currentScenario = new ExecutionInfo
            {
                CurrentScenario = scenarioInfo,
                CurrentSpec = specInfo
            };
            var currentExecutionInfo = new StepExecutionStartingRequest
            {
                CurrentExecutionInfo = currentScenario
            };

            var message = new Message
            {
                StepExecutionStartingRequest = currentExecutionInfo,
                MessageType = Message.Types.MessageType.ScenarioExecutionStarting,
                MessageId = 0
            };
            var tags = AssertEx.ExecuteProtectedMethod<StepExecutionStartingProcessor>("GetApplicableTags", message)
                .ToList();
            Assert.IsNotEmpty(tags);
            Assert.AreEqual(1, tags.Count);
            Assert.Contains("foo", tags);
        }
    }
}