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

using System;
using System.IO;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.Dotnet.IntegrationTests
{
    public class StubImplementationCodeTests
    {
        private readonly string _testProjectPath = TestUtils.GetIntegrationTestSampleDirectory();

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);
        }

        [Test]
        public void ShouldProcessMessage()
        {
            var message = new StubImplementationCodeRequest
            {
                ImplementationFilePath = "New File",
                Codes =
                    {
                        "method"
                    }
            };

            var processor = new StubImplementationCodeProcessor();
            var result = processor.Process(message);
            Assert.AreEqual("StepImplementation1.cs", Path.GetFileName(result.FilePath));
            Assert.AreEqual(1, result.TextDiffs.Count);
            Assert.True(result.TextDiffs[0].Content.Contains("namespace IntegrationTestSample"));
            Assert.True(result.TextDiffs[0].Content.Contains("class StepImplementation1"));
            Assert.AreEqual(result.TextDiffs[0].Span.Start, 0);
        }

        [Test]
        public void ShouldProcessMessageForExistingButEmptyFile()
        {
            var file = Path.Combine(TestUtils.GetIntegrationTestSampleDirectory(), "Empty.cs");
            var message = new StubImplementationCodeRequest
            {
                ImplementationFilePath = file,
                Codes =
                    {
                        "Step Method"
                    }
            };

            var processor = new StubImplementationCodeProcessor();
            var result = processor.Process(message);
            Assert.AreEqual(1, result.TextDiffs.Count);
            Assert.True(result.TextDiffs[0].Content.Contains("namespace IntegrationTestSample"));
            Assert.True(result.TextDiffs[0].Content.Contains("class Empty"));
            StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
            Assert.AreEqual(result.TextDiffs[0].Span.Start, 0);
        }

        [Test]
        public void ShouldProcessMessageForExistingClass()
        {
            var file = Path.Combine(TestUtils.GetIntegrationTestSampleDirectory(), "StepImplementation.cs");
            var message = new StubImplementationCodeRequest
            {
                ImplementationFilePath = file,
                Codes =
                    {
                        "Step Method"
                    }
            };

            var processor = new StubImplementationCodeProcessor();
            var result = processor.Process(message);
            Assert.AreEqual(1, result.TextDiffs.Count);
            StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
            Assert.AreEqual(result.TextDiffs[0].Span.Start, 115);
        }

        [Test]
        public void ShouldProcessMessageForExistingFileWithEmptyClass()
        {
            var file = Path.Combine(TestUtils.GetIntegrationTestSampleDirectory(), "EmptyClass.cs");
            var message = new StubImplementationCodeRequest
            {
                ImplementationFilePath = file,
                Codes =
                    {
                        "Step Method"
                    }
            };

            var processor = new StubImplementationCodeProcessor();
            var result = processor.Process(message);
            Assert.AreEqual(1, result.TextDiffs.Count);
            StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
            Assert.True(result.TextDiffs[0].Content.Contains("Step Method"));
            Assert.AreEqual(result.TextDiffs[0].Span.Start, 8);
        }

        [Test]
        public void ShouldProcessMessageForExistingFileWithSomeComments()
        {
            var file = Path.Combine(TestUtils.GetIntegrationTestSampleDirectory(), "CommentFile.cs");
            var message = new StubImplementationCodeRequest
                {
                    ImplementationFilePath = file,
                    Codes =
                    {
                        "Step Method"
                    }
                };

            var processor = new StubImplementationCodeProcessor();
            var result = processor.Process(message);
            Assert.AreEqual(1, result.TextDiffs.Count);
            StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
            Assert.True(result.TextDiffs[0].Content.Contains("namespace IntegrationTestSample"));
            Assert.True(result.TextDiffs[0].Content.Contains("class CommentFile"));
            Assert.AreEqual(result.TextDiffs[0].Span.Start, 3);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }
    }
}