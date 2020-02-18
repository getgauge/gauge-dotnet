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
using Gauge.CSharp.Core;
using Gauge.Dotnet.Extensions;

namespace Gauge.Dotnet
{
    public class SetupCommand : IGaugeCommand
    {
        void IGaugeCommand.Execute()
        {
            var gaugeProjectRoot = Utils.GaugeProjectRoot;
            var projName = new DirectoryInfo(gaugeProjectRoot).Name.ToValidCSharpIdentifier();

            var project = $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""FluentAssertions"" Version=""5.1.0"" />
    <PackageReference Include=""Gauge.CSharp.Lib"" Version=""0.7.5"" />
  </ItemGroup>

</Project>
";
            var properties = GenerateDefaultProperties(projName);

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
            Logger.Info("create  StepImplementation.cs");
            File.WriteAllText(Path.Combine(gaugeProjectRoot, "StepImplementation.cs"), implementation);

            Logger.Info($"create  {projName}.csproj");
            File.WriteAllText(Path.Combine(gaugeProjectRoot, $"{projName}.csproj"), project);

            var envPath = Path.Combine(gaugeProjectRoot, "env", "default");
            Directory.CreateDirectory(envPath);

            Logger.Info($"create  {Path.Combine("env", "default", "dotnet.properties")}");
            File.WriteAllText(Path.Combine(envPath, "dotnet.properties"), properties);
        }

        private static string GenerateDefaultProperties(string projName)
        {
            var properties = new[]
            {
                $"GAUGE_CSHARP_PROJECT_FILE={projName}.csproj",
                "GAUGE_CSHARP_PROJECT_CONFIG=release",
                Environment.NewLine,
                "# Possible values for this property are 'suite', 'spec' or 'scenario’.",
                "# 'scenario' clears the objects after the execution of each scenario, new objects are created for next execution.",
                "gauge_clear_state_level=scenario"
            };
            return string.Join(Environment.NewLine, properties);
        }
    }
}