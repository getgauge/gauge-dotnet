/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Extensions;
using Gauge.Messages;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.Dotnet.Processors;

public class StubImplementationCodeProcessor : IGaugeProcessor<StubImplementationCodeRequest, FileDiff>
{
    private readonly IConfiguration _config;

    public StubImplementationCodeProcessor(IConfiguration config)
    {
        _config = config;
    }

    public async Task<FileDiff> Process(int stream, StubImplementationCodeRequest request)
    {
        var stubs = request.Codes;
        var file = request.ImplementationFilePath;
        var response = new FileDiff();
        if (!File.Exists(file))
        {
            var filepath = GetFileName("", 0);
            ImplementInNewClass(response, filepath, stubs);
        }
        else
        {
            await ImlementInExistingFile(stubs, file, response);
        }

        return response;
    }

    private async Task ImlementInExistingFile(IEnumerable<string> stubs, string file, FileDiff response)
    {
        var content = await File.ReadAllTextAsync(file);
        if (content == "")
            ImplementInNewClass(response, file, stubs);
        else
            await ImplementInExistingClass(response, file, stubs);
    }

    private async Task ImplementInExistingClass(FileDiff response, string file, IEnumerable<string> stubs)
    {
        var text = await File.ReadAllTextAsync(file);
        var root = CSharpSyntaxTree.ParseText(text).GetRoot();
        var stepClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        var diff = new TextDiff();
        if (stepClass.Count() > 0)
        {
            diff = GetTextDiff(diff, stepClass, stubs);
        }
        else
        {
            diff = GetTextDiff(diff, root, stubs, file);
        }
        response.FilePath = file;
        response.TextDiffs.Add(diff);
    }

    private TextDiff GetTextDiff(TextDiff diff, SyntaxNode root, IEnumerable<string> stubs, string file)
    {
        var stepClassPosition = root.GetLocation().GetLineSpan().EndLinePosition;
        var className = GetClassName(file);

        diff.Span = new Span
        {
            Start = stepClassPosition.Line + 1,
            StartChar = stepClassPosition.Character,
            End = stepClassPosition.Line + 1,
            EndChar = stepClassPosition.Character
        };
        diff.Content = GetNewClassContent(className, stubs);
        return diff;
    }

    private TextDiff GetTextDiff(TextDiff diff, IEnumerable<ClassDeclarationSyntax> stepClass, IEnumerable<string> stubs)
    {
        var stepClassPosition = stepClass.First().GetLocation().GetLineSpan().EndLinePosition;
        diff.Span = new Span
        {
            Start = stepClassPosition.Line,
            StartChar = stepClassPosition.Character - 1,
            End = stepClassPosition.Line,
            EndChar = stepClassPosition.Character - 1
        };
        diff.Content = $"{Environment.NewLine}{string.Join(Environment.NewLine, stubs)}\t";
        return diff;
    }

    private void ImplementInNewClass(FileDiff fileDiff, string filepath, IEnumerable<string> stubs)
    {
        var className = GetClassName(filepath);
        var content = GetNewClassContent(className, stubs);
        var diff = new TextDiff
        {
            Span = new Span
            {
                Start = 0,
                StartChar = 0,
                End = 0,
                EndChar = 0
            },
            Content = content
        };
        fileDiff.TextDiffs.Add(diff);
        fileDiff.FilePath = filepath;
    }

    private string GetNewClassContent(string className, IEnumerable<string> stubs)
    {
        var n = Environment.NewLine;
        return $"using System;{n}" +
               $"using Gauge.CSharp.Lib.Attribute;{n}{n}" +
               $"namespace {GetNameSpace()}{n}" +
               $"{{\n\tpublic class {className}{n}" +
               $"\t{{{n}{string.Join(Environment.NewLine, stubs)}{n}" +
               $"\t}}{n}}}\n";
    }

    private string GetFileName(string suffix, int counter)
    {
        var fileName = Path.Combine(_config.GetGaugeProjectRoot(), $"StepImplementation{suffix}.cs");
        return !File.Exists(fileName) ? fileName : GetFileName((++counter).ToString(), counter);
    }

    public string GetNameSpace()
    {
        var gaugeProjectRoot = _config.GetGaugeProjectRoot();
        return new DirectoryInfo(gaugeProjectRoot).Name.ToValidCSharpIdentifier();
    }

    private static string GetClassName(string filepath)
    {
        return Path.GetFileNameWithoutExtension(filepath);
    }

}