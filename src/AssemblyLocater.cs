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
using Gauge.Dotnet.Wrappers;

namespace Gauge.Dotnet
{
    public class AssemblyLocater : IAssemblyLocater
    {
        private readonly IDirectoryWrapper _directoryWrapper;

        private readonly IFileWrapper _fileWrapper;

        public AssemblyLocater(IDirectoryWrapper directoryWrapper, IFileWrapper fileWrapper)
        {
            _directoryWrapper = directoryWrapper;
            _fileWrapper = fileWrapper;
        }

        public IEnumerable<string> GetAllAssemblies()
        {
            var assemblies = _directoryWrapper
                .EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly)
                .ToList();
            var gaugeAdditionalLibsPath = Environment.GetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS");
            if (string.IsNullOrEmpty(gaugeAdditionalLibsPath))
                return assemblies;

            var additionalLibPaths = gaugeAdditionalLibsPath.Split(',').Select(s => Path.GetFullPath(s.Trim()));
            foreach (var libPath in additionalLibPaths)
            {
                if (Path.HasExtension(libPath))
                {
                    AddFile(libPath, assemblies);
                    continue;
                }

                AddFilesFromDirectory(libPath, assemblies);
            }

            return assemblies;
        }

        private void AddFilesFromDirectory(string path, List<string> assemblies)
        {
            if (!_directoryWrapper.Exists(path))
                return;
            assemblies.AddRange(_directoryWrapper.EnumerateFiles(path, "*.dll", SearchOption.TopDirectoryOnly));
        }

        private void AddFile(string path, List<string> assemblies)
        {
            if (!_fileWrapper.Exists(path))
                return;
            assemblies.Add(path);
        }
    }
}