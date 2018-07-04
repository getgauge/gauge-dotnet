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

using System.Collections.Generic;
using System.IO;
using Gauge.Dotnet.Helpers;
using Gauge.Messages;

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
                ImplementInNewClass(response,filepath, stubs);
            }
            else
            {
                var content = File.ReadAllText(file);
                if (content == "")
                {
                   ImplementInNewClass(response, file, stubs);
                }
            }
            return new Message{FileDiff = response};
        }

        private static void ImplementInNewClass(FileDiff fileDiff,string filepath, IEnumerable<string> stubs)
        {
            var className = FileHelper.GetClassName(filepath);
            var content = ImplementationHelper.CreateImplementationInNewClass(className, stubs);
            var item = new TextDiff
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
            fileDiff.TextDiffs.Add(item);
            fileDiff.FilePath = filepath;
        }
    }
}