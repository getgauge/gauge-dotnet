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