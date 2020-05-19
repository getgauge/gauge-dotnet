/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gauge.Dotnet.Helpers;
using Gauge.Messages;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.Dotnet.Processors
{
    public class StubImplementationCodeProcessor
    {
        public FileDiff Process(StubImplementationCodeRequest request)
        {
            var stubs = request.Codes;
            var file = request.ImplementationFilePath;
            var response = new FileDiff();
            if (!File.Exists(file))
            {
                var filepath = FileHelper.GetFileName("", 0);
                ImplementInNewClass(response, filepath, stubs);
            }
            else
            {
                ImlementInExistingFile(stubs, file, response);
            }

            return response;
        }

        private void ImlementInExistingFile(IEnumerable<string> stubs, string file, FileDiff response)
        {
            var content = File.ReadAllText(file);
            if (content == "")
                ImplementInNewClass(response, file, stubs);
            else
                ImplementInExistingClass(response, file, stubs);
        }

        private void ImplementInExistingClass(FileDiff response, string file, IEnumerable<string> stubs)
        {
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();
            var stepClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var diff = new TextDiff();
            if (hasStepClass(stepClass))
            {
                diff = getTextDiff(diff, stepClass, stubs);
            }
            else
            {
                diff = getTextDiff(diff, root, stubs, file);
            }
            response.FilePath = file;
            response.TextDiffs.Add(diff);
        }

        private TextDiff getTextDiff(TextDiff diff, SyntaxNode root, IEnumerable<string> stubs, string file)
        {
            var stepClassPosition = root.GetLocation().GetLineSpan().EndLinePosition;
            var className = FileHelper.GetClassName(file);

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

        private bool hasStepClass(IEnumerable<ClassDeclarationSyntax> stepClass)
        {
            return stepClass.ToList().Count() > 0;
        }

        private TextDiff getTextDiff(TextDiff diff, IEnumerable<ClassDeclarationSyntax> stepClass, IEnumerable<string> stubs)
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
            var className = FileHelper.GetClassName(filepath);
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
                   $"namespace {FileHelper.GetNameSpace()}{n}" +
                   $"{{\n\tpublic class {className}{n}" +
                   $"\t{{{n}{string.Join(Environment.NewLine, stubs)}{n}" +
                   $"\t}}{n}}}\n";
        }
    }
}