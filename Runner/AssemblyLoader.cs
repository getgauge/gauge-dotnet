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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Runner.Wrappers;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private const string GaugeLibAssembleName = "Gauge.CSharp.Lib";
        private readonly IAssemblyWrapper _assemblyWrapper;

        public AssemblyLoader(IAssemblyWrapper assemblyWrapper, IEnumerable<string> assemblyLocations)
        {
            _assemblyWrapper = assemblyWrapper;
            AssembliesReferencingGaugeLib = new List<Assembly>();
            ScreengrabberTypes = new List<Type>();
            ClassInstanceManagerTypes = new List<Type>();
            foreach (var location in assemblyLocations)
                ScanAndLoad(location);
        }

        public AssemblyLoader(IEnumerable<string> assemblyLocations)
            : this(new AssemblyWrapper(), assemblyLocations)
        {
        }

        public AssemblyLoader()
            : this(new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies())
        {
        }

        public List<Assembly> AssembliesReferencingGaugeLib { get; }
        public List<Type> ScreengrabberTypes { get; }
        public List<Type> ClassInstanceManagerTypes { get; }

        public List<MethodInfo> GetMethods(string annotation)
        {
            Func<MethodInfo, bool> methodFilter = info => info.GetCustomAttributes()
                .Any(a => a.GetType().FullName.Equals(annotation));
            Func<Type, IEnumerable<MethodInfo>> methodSelector = t => t.GetMethods().Where(methodFilter);
            return AssembliesReferencingGaugeLib.SelectMany(assembly => assembly.GetTypes().SelectMany(methodSelector))
                .ToList();
        }

        private void ScanAndLoad(string path)
        {
            var logger = LogManager.GetLogger("AssemblyLoader");
            logger.Debug("Loading assembly from : {0}", path);
            Assembly assembly;
            try
            {
                assembly = _assemblyWrapper.LoadFrom(path);
            }
            catch
            {
                logger.Warn("Failed to scan assembly {0}", path);
                return;
            }

            var isReferencingGaugeLib = assembly.GetReferencedAssemblies()
                .Select(name => name.Name)
                .Contains(GaugeLibAssembleName);

            var loadableTypes = new HashSet<Type>(isReferencingGaugeLib ? GetLoadableTypes(assembly) : new Type[] { });

            // Load assembly so that code can be executed
            var fullyLoadedAssembly = _assemblyWrapper.LoadFrom(path);
            var types = GetFullyLoadedTypes(loadableTypes, fullyLoadedAssembly).ToList();

            if (isReferencingGaugeLib)
                AssembliesReferencingGaugeLib.Add(fullyLoadedAssembly);

            ScanForScreengrabber(types);
            ScanForInstanceManager(types);
        }

        private IEnumerable<Type> GetFullyLoadedTypes(IEnumerable<Type> loadableTypes, Assembly fullyLoadedAssembly)
        {
            foreach (var type in loadableTypes)
            {
                var fullyLoadedType = fullyLoadedAssembly.GetType(type.FullName);
                if (fullyLoadedType != null)
                    yield return fullyLoadedType;
            }
        }

        private void ScanForScreengrabber(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == typeof(IScreenGrabber).FullName));
            ScreengrabberTypes.AddRange(implementingTypes);
        }

        private void ScanForInstanceManager(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == typeof(IClassInstanceManager).FullName));
            ClassInstanceManagerTypes.AddRange(implementingTypes);
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                LogManager.GetLogger("AssemblyLoader")
                    .Warn("Could not scan all types in assembly {0}", assembly.CodeBase);
                return e.Types.Where(type => type != null);
            }
        }
    }
}