/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.IO;
using System.Linq;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Exceptions;
using Gauge.Dotnet.Wrappers;

namespace Gauge.Dotnet
{
    public class AssemblyLocater : IAssemblyLocater
    {
        private readonly IDirectoryWrapper _directoryWrapper;

        public AssemblyLocater(IDirectoryWrapper directoryWrapper)
        {
            _directoryWrapper = directoryWrapper;
        }

        public AssemblyPath GetTestAssembly()
        {
            var gaugeBinDir = Utils.GetGaugeBinDir();
            try
            {
                return _directoryWrapper
                    .EnumerateFiles(gaugeBinDir, "*.deps.json", SearchOption.TopDirectoryOnly)
                    .First().Replace(".deps.json", ".dll");
            }
            catch (System.InvalidOperationException)
            {
                throw new GaugeTestAssemblyNotFoundException(gaugeBinDir);
            }
        }
    }
}