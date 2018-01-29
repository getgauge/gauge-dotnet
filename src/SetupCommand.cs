// Copyright 2015 ThoughtWorks, Inc.
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

using Gauge.CSharp.Core;
using System.IO;

namespace Gauge.Dotnet
{
    public class SetupCommand : IGaugeCommand
    {
        void IGaugeCommand.Execute()
        {
            string gaugeProjectRoot = Utils.GaugeProjectRoot;
            var projName = new DirectoryInfo(gaugeProjectRoot).Name;

            var implementation = $@"using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

namespace {projName}
{{
    public class StepImplementation
    {{
        private HashSet<char> _vowels;

        [Step(""Vowels in English language are <vowelString>."")]
        public void SetLanguageVowels(string vowelString)
        {{
            _vowels = new HashSet<char>();
            foreach (var c in vowelString)
            {{
                _vowels.Add(c);
            }}
        }}

        [Step(""The word <word> has <expectedCount> vowels."")]
        public void VerifyVowelsCountInWord(string word, int expectedCount)
        {{
            var actualCount = CountVowels(word);
            actualCount.Should().Be(expectedCount);
        }}

        [Step(""Almost all words have vowels <wordsTable>"")]
        public void VerifyVowelsCountInMultipleWords(Table wordsTable)
        {{
            var rows = wordsTable.GetTableRows();
            foreach (var row in rows)
            {{
                var word = row.GetCell(""Word"");
                var expectedCount = Convert.ToInt32(row.GetCell(""Vowel Count""));
                var actualCount = CountVowels(word);

                actualCount.Should().Be(expectedCount);
            }}
        }}

        private int CountVowels(string word)
        {{
            return word.Count(c => _vowels.Contains(c));
        }}
    }}
}}";
            File.WriteAllText(Path.Combine(gaugeProjectRoot, "StepImplementation.cs"), implementation);
            // dotnet new solution
            GaugeProjectBuilder.RunDotnetCommand("new solution");
            // dotnet new classlib
            GaugeProjectBuilder.RunDotnetCommand("new classlib --no-restore");
            // remove Class1.cs
            File.Delete(Path.Combine(gaugeProjectRoot, "Class1.cs"));
            // add references to project
            GaugeProjectBuilder.RunDotnetCommand($"add {projName}.csproj package Gauge.CSharp.Lib");
            GaugeProjectBuilder.RunDotnetCommand($"add {projName}.csproj package FluentAssertions");
            // add project to sln
            GaugeProjectBuilder.RunDotnetCommand($"sln {projName}.sln add {projName}.csproj");
            return;
        }
    }
}