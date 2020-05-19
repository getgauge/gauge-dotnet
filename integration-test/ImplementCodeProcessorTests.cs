/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


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
            Console.WriteLine(result.TextDiffs[0].Content);
            Assert.True(result.TextDiffs[0].Content.Contains("namespace Sample"));
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
            Assert.True(result.TextDiffs[0].Content.Contains("namespace Sample"));
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
            Assert.AreEqual(131, result.TextDiffs[0].Span.Start);
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
            Assert.True(result.TextDiffs[0].Content.Contains("namespace Sample"));
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