/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Extensions;

namespace Gauge.Dotnet.Helpers;

public static class FileHelper
{
    public static IEnumerable<string> GetRemovedDirFiles(IConfiguration config)
    {
        var removedFiles = new List<string>();
        var excludedDirs = config.GetGaugeExcludeDirs();
        if (excludedDirs == null) return removedFiles;

        var excludedDir = excludedDirs.Split(",").Select(dir => dir.Trim()).ToList();
        foreach (var dir in excludedDir)
        {
            var dirpath = Path.Combine(config.GetGaugeProjectRoot(), dir);
            if (!Directory.Exists(dirpath)) continue;
            removedFiles.AddRange(Directory.EnumerateFiles(dirpath, "*.cs",
                SearchOption.AllDirectories));
        }
        return removedFiles;
    }
}