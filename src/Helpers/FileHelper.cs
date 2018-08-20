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


using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Extensions;

namespace Gauge.Dotnet.Helpers
{
    public class FileHelper
    {
        public static IEnumerable<string> GetImplementationFiles()
        {
            var di = new DirectoryInfo(Utils.GaugeProjectRoot);
            var allTopLevelDirs = di.GetDirectories("*", SearchOption.TopDirectoryOnly);
            var projectDirs = new List<string> {"bin", "logs", "env", "gauge_bin", "obj", "reports"};
            var testDirs = allTopLevelDirs.Where(dir => !projectDirs.Contains(dir.Name) && !dir.Name.StartsWith("."))
                .Select(dir => dir.FullName);

            var classFiles = Directory.EnumerateFiles(Utils.GaugeProjectRoot, "*.cs", SearchOption.TopDirectoryOnly)
                .ToList();


            foreach (var dir in testDirs)
            {
                var files = Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories);
                classFiles.AddRange(files);
            }

            return classFiles;
        }

        public static string GetImplementationGlobPatterns()
        {
            return $"{Utils.GaugeProjectRoot}/**/*.cs";
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