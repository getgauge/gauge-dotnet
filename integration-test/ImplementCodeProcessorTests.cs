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
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.IntegrationTests
{
    public class StubImplementationCodeTests : IntegrationTestsBase
    {
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
            ClassicAssert.AreEqual("StepImplementation1.cs", Path.GetFileName(result.FilePath));
            ClassicAssert.AreEqual(1, result.TextDiffs.Count);
            Console.WriteLine(result.TextDiffs[0].Content);
            ClassicAssert.True(result.TextDiffs[0].Content.Contains("namespace Sample"));
            ClassicAssert.True(result.TextDiffs[0].Content.Contains("class StepImplementation1"));
            ClassicAssert.AreEqual(result.TextDiffs[0].Span.Start, 0);
        }

        [Test]
        public void ShouldProcessMessageForExistingButEmptyFile()
        {
            var file = Path.Combine(_testProjectPath, "Empty.cs");
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
            ClassicAssert.AreEqual(1, result.TextDiffs.Count);
            ClassicAssert.True(result.TextDiffs[0].Content.Contains("namespace Sample"));
            ClassicAssert.True(result.TextDiffs[0].Content.Contains("class Empty"));
            StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
            ClassicAssert.AreEqual(result.TextDiffs[0].Span.Start, 0);
        }

        [Test]
        public void ShouldProcessMessageForExistingClass()
        {
            var file = Path.Combine(_testProjectPath, "StepImplementation.cs");
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
            ClassicAssert.AreEqual(1, result.TextDiffs.Count);
            StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
            ClassicAssert.AreEqual(115, result.TextDiffs[0].Span.Start);
        }

        [Test]
        public void ShouldProcessMessageForExistingFileWithEmptyClass()
        {
            var file = Path.Combine(_testProjectPath, "EmptyClass.cs");
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
            ClassicAssert.AreEqual(1, result.TextDiffs.Count);
            StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
            ClassicAssert.True(result.TextDiffs[0].Content.Contains("Step Method"));
            ClassicAssert.AreEqual(result.TextDiffs[0].Span.Start, 8);
        }

        [Test]
        public void ShouldProcessMessageForExistingFileWithSomeComments()
        {
            var file = Path.Combine(_testProjectPath, "CommentFile.cs");
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
            ClassicAssert.AreEqual(1, result.TextDiffs.Count);
            StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
            ClassicAssert.True(result.TextDiffs[0].Content.Contains("namespace Sample"));
            ClassicAssert.True(result.TextDiffs[0].Content.Contains("class CommentFile"));
            ClassicAssert.AreEqual(result.TextDiffs[0].Span.Start, 3);
        }
    }
}