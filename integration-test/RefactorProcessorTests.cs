/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.IntegrationTests
{
    public class RefactorProcessorTests
    {
        private readonly string _testProjectPath = TestUtils.GetIntegrationTestSampleDirectory();

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);

            File.Copy(Path.Combine(_testProjectPath, "RefactoringSample.cs"),
                Path.Combine(_testProjectPath, "RefactoringSample.copy1"), true);
        }

        [Test]
        public void ShouldAddParameters()
        {
            const string parameterizedStepText = "Refactoring 1 Say <what> to <who>";
            const string stepValue = "Refactoring 1 Say {} to {}";
            var stepRegistry = new StepRegistry();
            stepRegistry.AddStep(stepValue, new GaugeMethod
            {
                Name = "RefactoringSaySomething1",
                ClassName = "RefactoringSample",
                FileName = Path.Combine(_testProjectPath, "RefactoringSample.cs"),
                StepText = parameterizedStepText,
                StepValue = stepValue
            });
            var message = new RefactorRequest
            {
                OldStepValue = new ProtoStepValue
                {
                    StepValue = stepValue,
                    ParameterizedStepValue = parameterizedStepText,
                    Parameters = { "what", "who" }
                },
                NewStepValue = new ProtoStepValue
                {
                    StepValue = "Refactoring 1 Say {} to {} at {}",
                    ParameterizedStepValue = "Refactoring 1 Say <what> to <who> at <when>",
                    Parameters = { "who", "what", "when" }
                },
                ParamPositions =
                    {
                        new ParameterPosition {OldPosition = 0, NewPosition = 0},
                        new ParameterPosition {OldPosition = 1, NewPosition = 1},
                        new ParameterPosition {OldPosition = -1, NewPosition = 2}
                    }
            };

            var refactorProcessor = new RefactorProcessor(stepRegistry);
            var result = refactorProcessor.Process(message);
            ClassicAssert.IsTrue(result.Success);
        }

        [TearDown]
        public void TearDown()
        {
            var sourceFileName = Path.Combine(_testProjectPath, "RefactoringSample.copy1");
            File.Copy(sourceFileName, Path.Combine(_testProjectPath, "RefactoringSample.cs"), true);
            File.Delete(sourceFileName);
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }
    }
}