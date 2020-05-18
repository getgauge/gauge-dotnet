/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Gauge.Dotnet.Wrappers
{
    public class AssemblyWrapper : IAssemblyWrapper
    {
        public Assembly[] GetCurrentDomainAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public Assembly LoadFrom(string location)
        {
            using (var stream = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var symbolFile = Path.ChangeExtension(location, "pdb");
                if (File.Exists(symbolFile))
                {
                    var symbolStream = new FileStream(symbolFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    return AssemblyLoadContext.Default.LoadFromStream(stream, symbolStream);
                }

                return AssemblyLoadContext.Default.LoadFromStream(stream);
            }
        }
    }
}