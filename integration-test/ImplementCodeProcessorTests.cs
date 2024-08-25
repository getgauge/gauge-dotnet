/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Processors;
using Gauge.Messages;

namespace Gauge.Dotnet.IntegrationTests;

public class StubImplementationCodeTests : IntegrationTestsBase
{
    [Test]
    public async Task ShouldProcessMessage()
    {
        var message = new StubImplementationCodeRequest
        {
            ImplementationFilePath = "New File",
            Codes =
                {
                    "method"
                }
        };

        var processor = new StubImplementationCodeProcessor(_configuration);
        var result = await processor.Process(message);
        ClassicAssert.AreEqual("StepImplementation1.cs", Path.GetFileName(result.FilePath));
        ClassicAssert.AreEqual(1, result.TextDiffs.Count);
        Console.WriteLine(result.TextDiffs[0].Content);
        ClassicAssert.True(result.TextDiffs[0].Content.Contains("namespace Sample"));
        ClassicAssert.True(result.TextDiffs[0].Content.Contains("class StepImplementation1"));
        ClassicAssert.AreEqual(result.TextDiffs[0].Span.Start, 0);
    }

    [Test]
    public async Task ShouldProcessMessageForExistingButEmptyFile()
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

        var processor = new StubImplementationCodeProcessor(_configuration);
        var result = await processor.Process(message);
        ClassicAssert.AreEqual(1, result.TextDiffs.Count);
        ClassicAssert.True(result.TextDiffs[0].Content.Contains("namespace Sample"));
        ClassicAssert.True(result.TextDiffs[0].Content.Contains("class Empty"));
        StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
        ClassicAssert.AreEqual(result.TextDiffs[0].Span.Start, 0);
    }

    [Test]
    public async Task ShouldProcessMessageForExistingClass()
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

        var processor = new StubImplementationCodeProcessor(_configuration);
        var result = await processor.Process(message);
        ClassicAssert.AreEqual(1, result.TextDiffs.Count);
        StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
        ClassicAssert.AreEqual(107, result.TextDiffs[0].Span.Start);
    }

    [Test]
    public async Task ShouldProcessMessageForExistingFileWithEmptyClass()
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

        var processor = new StubImplementationCodeProcessor(_configuration);
        var result = await processor.Process(message);
        ClassicAssert.AreEqual(1, result.TextDiffs.Count);
        StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
        ClassicAssert.True(result.TextDiffs[0].Content.Contains("Step Method"));
        ClassicAssert.AreEqual(result.TextDiffs[0].Span.Start, 8);
    }

    [Test]
    public async Task ShouldProcessMessageForExistingFileWithSomeComments()
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

        var processor = new StubImplementationCodeProcessor(_configuration);
        var result = await processor.Process(message);
        ClassicAssert.AreEqual(1, result.TextDiffs.Count);
        StringAssert.Contains("Step Method", result.TextDiffs[0].Content);
        ClassicAssert.True(result.TextDiffs[0].Content.Contains("namespace Sample"));
        ClassicAssert.True(result.TextDiffs[0].Content.Contains("class CommentFile"));
        ClassicAssert.AreEqual(result.TextDiffs[0].Span.Start, 3);
    }
}