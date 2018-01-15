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
using Gauge.CSharp.Runner.Wrappers;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private const string GaugeLibAssembleName = "Gauge.CSharp.Lib";
        private readonly IAssemblyWrapper _assemblyWrapper;
        private Assembly _targetLibAssembly;

        public AssemblyLoader(IAssemblyWrapper assemblyWrapper, IEnumerable<string> assemblyLocations)
        {
            _assemblyWrapper = assemblyWrapper;
            AssembliesReferencingGaugeLib = new List<Assembly>();
            foreach (var location in assemblyLocations)
                ScanAndLoad(location);

            LoadTargetLibAssembly();

            ScreengrabberType = ScreengrabberType ?? _targetLibAssembly.GetType(LibType.DefaultScreenGrabber.FullName());
            ClassInstanceManagerType = ClassInstanceManagerType ?? _targetLibAssembly.GetType(LibType.DefaultClassInstanceManager.FullName());
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
        public Type ScreengrabberType { get; private set; }
        public Type ClassInstanceManagerType { get; private set; }

        public IEnumerable<MethodInfo> GetMethods(LibType type)
        {
            var attributeType = _targetLibAssembly.GetType(type.FullName());
            bool methodFilter(MethodInfo info) => info.GetCustomAttributes(false)
                .Any(attributeType.IsInstanceOfType);
            IEnumerable<MethodInfo> methodSelector(Type t) => t.GetMethods().Where(methodFilter);
            return AssembliesReferencingGaugeLib.SelectMany(assembly => assembly.GetTypes().SelectMany(methodSelector));
        }

        private void ScanAndLoad(string path)
        {
            var logger = LogManager.GetLogger("AssemblyLoader");
            logger.Debug("Loading assembly from : {0}", path);
            Assembly assembly = _assemblyWrapper.LoadFrom(path);
            
            var isReferencingGaugeLib = assembly.GetReferencedAssemblies()
                .Select(name => name.Name)
                .Contains(GaugeLibAssembleName);

            var loadableTypes = new HashSet<Type>(isReferencingGaugeLib ? GetLoadableTypes(assembly) : new Type[] { });

            // Load assembly so that code can be executed
            var fullyLoadedAssembly = _assemblyWrapper.LoadFrom(path);
            var types = GetFullyLoadedTypes(loadableTypes, fullyLoadedAssembly).ToList();

            if (isReferencingGaugeLib)
                AssembliesReferencingGaugeLib.Add(fullyLoadedAssembly);

            if (ScreengrabberType is null)
                ScanForScreengrabber(types);

            if (ClassInstanceManagerType is null)
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
                type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.IScreenGrabber"));
            ScreengrabberType=implementingTypes.FirstOrDefault();
        }

        private void ScanForInstanceManager(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.IClassInstanceManager"));
            ClassInstanceManagerType = implementingTypes.FirstOrDefault();
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

        private void LoadTargetLibAssembly()
        {            
            _targetLibAssembly = _assemblyWrapper.GetCurrentDomainAssemblies().First(x => string.CompareOrdinal(x.GetName().Name, GaugeLibAssembleName) == 0);
        }

        public Type GetLibType(LibType type)
        {
            return _targetLibAssembly.GetType(type.FullName());
        }
    }
}