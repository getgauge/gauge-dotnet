// Copyright 2018 ThoughtWorks, Inc.
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
using System.IO;
using System.Linq;
using Gauge.Dotnet.Helpers;
using Gauge.Messages;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.Dotnet.Processors
{
    public class StubImplementationCodeProcessor : IMessageProcessor
    {
        public Message Process(Message request)
        {
            var stubs = request.StubImplementationCodeRequest.Codes;
            var file = request.StubImplementationCodeRequest.ImplementationFilePath;
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

            return new Message {FileDiff = response};
        }

        private static void ImlementInExistingFile(IEnumerable<string> stubs, string file, FileDiff response)
        {
            var content = File.ReadAllText(file);
            if (content == "")
                ImplementInNewClass(response, file, stubs);
            else
                ImplementInExistingClass(response, file, stubs);
        }

        private static void ImplementInExistingClass(FileDiff response, string file, IEnumerable<string> stubs)
        {
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();
            var stepMethods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var lastMethodEndPosition = stepMethods.Last().GetLocation().GetLineSpan().EndLinePosition;
            var diff = new TextDiff
            {
                Span = new Span
                {
                    Start = lastMethodEndPosition.Line,
                    StartChar = lastMethodEndPosition.Character + 1,
                    End = lastMethodEndPosition.Line,
                    EndChar = lastMethodEndPosition.Character + 1
                },
                Content = $"{Environment.NewLine}{string.Join(Environment.NewLine, stubs)}"
            };

            response.FilePath = file;
            response.TextDiffs.Add(diff);
        }

        private static void ImplementInNewClass(FileDiff fileDiff, string filepath, IEnumerable<string> stubs)
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

        private static string GetNewClassContent(string className, IEnumerable<string> stubs)
        {
            var n = Environment.NewLine;
            return $"using System;{n}" +
                   $"using Gauge.CSharp.Lib.Attribute;{n}{n}" +
                   $"namespace {FileHelper.GetNameSpace()}{n}" +
                   $"{{\n\tpublic class {className}{n}" +
                   $"\t\t{{{n}{string.Join(Environment.NewLine, stubs)}{n}" +
                   $"\t}}{n}}}\n";
        }
    }
}