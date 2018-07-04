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
using Gauge.CSharp.Core;
using Gauge.Dotnet.Extensions;

namespace Gauge.Dotnet.Helpers
{
    public class FileHelper
    {
        public static IEnumerable<string> GetImplementationFiles()
        {
            var classFiles = Directory.EnumerateFiles(Utils.GaugeProjectRoot, "*.cs",
                SearchOption.AllDirectories);
            return classFiles;
        }

        public static string GetImplementationGlobPatterns()
        {
            return Path.Combine(Utils.GaugeProjectRoot, "**", "*.cs");
        }

        public static string GetNameSpace()
        {
            var gaugeProjectRoot = Utils.GaugeProjectRoot;
            return new DirectoryInfo(gaugeProjectRoot).Name.ToValidCSharpIdentifier();
        }

        public static string GetFileName(string suffix, int counter)
        {
            var fileName = Path.Combine(Utils.GaugeProjectRoot, $"StepImplementation{suffix}.cs");
            return !File.Exists(fileName) ? fileName : GetFileName((++counter).ToString(), counter);
        }

        public static string GetClassName(string filepath)
        {
            return Path.GetFileNameWithoutExtension(filepath);
        }
    }
}