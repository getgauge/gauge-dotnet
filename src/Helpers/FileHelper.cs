/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/



using System;
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
            var classFiles = Directory.EnumerateFiles(Utils.GaugeProjectRoot, "*.cs", SearchOption.AllDirectories)
                .ToList();

            var attributes = new AttributesLoader().GetRemovedAttributes();
            foreach (var attribute in attributes)
                classFiles.Remove(Path.Combine(Utils.GaugeProjectRoot, attribute.Value));

            var removedFiles = GetRemovedDirFiles();
            return classFiles.Except(removedFiles);
        }

        public static IEnumerable<string> GetRemovedDirFiles()
        {
            var removedFiles = new List<string>();
            var excludedDirs = Environment.GetEnvironmentVariable("gauge_exclude_dirs");
            if (excludedDirs == null) return removedFiles;

            var excludedDir = excludedDirs.Split(",").Select(dir => dir.Trim()).ToList();
            foreach (var dir in excludedDir)
            {
                var dirpath = Path.Combine(Utils.GaugeProjectRoot, dir);
                if (!Directory.Exists(dirpath)) continue;
                removedFiles.AddRange(Directory.EnumerateFiles(dirpath, "*.cs",
                    SearchOption.AllDirectories));
            }
            return removedFiles;
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