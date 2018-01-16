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
        private readonly IReflectionWrapper _reflectionWrapper;
        private Assembly _targetLibAssembly;

        public AssemblyLoader(IAssemblyWrapper assemblyWrapper, IEnumerable<string> assemblyLocations, IReflectionWrapper reflectionWrapper)
        {
            _assemblyWrapper = assemblyWrapper;
            _reflectionWrapper = reflectionWrapper;
            AssembliesReferencingGaugeLib = new List<Assembly>();
            foreach (var location in assemblyLocations)
                ScanAndLoad(location);

            LoadTargetLibAssembly();
        }

        public List<Assembly> AssembliesReferencingGaugeLib { get; }
        public Type ScreengrabberType { get; private set; }
        public Type ClassInstanceManagerType { get; private set; }

        public IEnumerable<MethodInfo> GetMethods(LibType type)
        {
            var attributeType = _targetLibAssembly.GetType(type.FullName());
            bool methodFilter(MethodInfo info) => info.GetCustomAttributes(false)
                .Any(attributeType.IsInstanceOfType);
            IEnumerable<MethodInfo> methodSelector(Type t) => _reflectionWrapper.GetMethods(t).Where(methodFilter);
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

            if (!isReferencingGaugeLib)
                return;

            AssembliesReferencingGaugeLib.Add(assembly);
            var types = assembly.GetTypes();

            if (ScreengrabberType is null)
                ScanForScreengrabber(types);

            if (ClassInstanceManagerType is null)
                ScanForInstanceManager(types);
        }

        private void ScanForScreengrabber(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.IScreenGrabber"));
            ScreengrabberType = implementingTypes.FirstOrDefault() ?? _targetLibAssembly.GetType(LibType.DefaultScreenGrabber.FullName());
        }

        private void ScanForInstanceManager(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.IClassInstanceManager"));
            ClassInstanceManagerType = implementingTypes.FirstOrDefault() ?? _targetLibAssembly.GetType(LibType.DefaultClassInstanceManager.FullName());
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