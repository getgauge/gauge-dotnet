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
using System.Text;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Strategy;
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
            var mockExectionHelper = new Mock<IExecutionOrchestrator>();

            var request = new StepExecutionStartingRequest
            {
                CurrentExecutionInfo = new ExecutionInfo
                {
                    CurrentSpec = new SpecInfo(),
                    CurrentScenario = new ScenarioInfo()
                }
            };

            var protoExecutionResult = new ProtoExecutionResult { ExecutionTime = 0, Failed = false };
            mockExectionHelper.Setup(executor =>
                    executor.ExecuteHooks(It.IsAny<string>(), It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(),
                        It.IsAny<ExecutionContext>()))
                .Returns(protoExecutionResult);
            var hookRegistry = new Mock<IHookRegistry>();
            hookRegistry.Setup(registry => registry.BeforeStepHooks).Returns(new HashSet<IHookMethod>());

            var pendingMessages = new List<string> { "one", "two" };
            var pendingScreenshots = new List<byte[]> { Encoding.ASCII.GetBytes("screenshot") };

            mockExectionHelper.Setup(x =>
                    x.ExecuteHooks("BeforeStep", It.IsAny<HooksStrategy>(), It.IsAny<IList<string>>(),
                        It.IsAny<ExecutionContext>()))
                .Returns(protoExecutionResult);
            mockExectionHelper.Setup(x =>
                x.GetAllPendingMessages()).Returns(pendingMessages);
            mockExectionHelper.Setup(x =>
                x.GetAllPendingScreenshots()).Returns(pendingScreenshots);

            var processor = new StepExecutionStartingProcessor(mockExectionHelper.Object);
            var result = processor.Process(request);
            Assert.AreEqual(result.ExecutionResult.Message, pendingMessages);
            Assert.AreEqual(result.ExecutionResult.Screenshots, pendingScreenshots);
        }

        [Test]
        public void ShouldGetTagListFromScenarioAndSpec()
        {
            var specInfo = new SpecInfo
            {
                Tags = { "foo" },
                Name = "",
                FileName = "",
                IsFailed = false
            };
            var scenarioInfo = new ScenarioInfo
            {
                Tags = { "bar" },
                Name = "",
                IsFailed = false
            };
            var currentScenario = new ExecutionInfo
            {
                CurrentScenario = scenarioInfo,
                CurrentSpec = specInfo
            };

            var tags = AssertEx.ExecuteProtectedMethod<StepExecutionStartingProcessor>("GetApplicableTags", currentScenario)
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
                Tags = { "foo" },
                Name = "",
                FileName = "",
                IsFailed = false
            };
            var scenarioInfo = new ScenarioInfo
            {
                Tags = { "foo" },
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

            var tags = AssertEx.ExecuteProtectedMethod<StepExecutionStartingProcessor>("GetApplicableTags", currentScenario)
                .ToList();
            Assert.IsNotEmpty(tags);
            Assert.AreEqual(1, tags.Count);
            Assert.Contains("foo", tags);
        }
    }
}