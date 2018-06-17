// Copyright 2018 ThoughtWorks, Inc.
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
using System.Text.RegularExpressions;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;
using NLog;

namespace Gauge.Dotnet
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private const string GaugeLibAssembleName = "Gauge.CSharp.Lib";
        private readonly IAssemblyWrapper _assemblyWrapper;
        private readonly IReflectionWrapper _reflectionWrapper;
        private Assembly _targetLibAssembly;

        public AssemblyLoader(IAssemblyWrapper assemblyWrapper, IEnumerable<string> assemblyLocations,
            IReflectionWrapper reflectionWrapper)
        {
            _assemblyWrapper = assemblyWrapper;
            _reflectionWrapper = reflectionWrapper;
            AssembliesReferencingGaugeLib = new List<Assembly>();
            foreach (var location in assemblyLocations)
                ScanAndLoad(location);

            LoadTargetLibAssembly();
            SetDefaultTypes();
        }

        public List<Assembly> AssembliesReferencingGaugeLib { get; }
        public Type ScreengrabberType { get; private set; }
        public Type ClassInstanceManagerType { get; private set; }

        public IEnumerable<MethodInfo> GetMethods(LibType type)
        {
            var attributeType = _targetLibAssembly.GetType(type.FullName());

            bool MethodFilter(MethodInfo info)
            {
                return info.GetCustomAttributes(false)
                    .Any(attributeType.IsInstanceOfType);
            }

            IEnumerable<MethodInfo> MethodSelector(Type t)
            {
                return _reflectionWrapper.GetMethods(t).Where(MethodFilter);
            }

            return AssembliesReferencingGaugeLib.SelectMany(assembly => assembly.GetTypes().SelectMany(MethodSelector));
        }

        public Type GetLibType(LibType type)
        {
            return _targetLibAssembly.GetType(type.FullName());
        }


        public IStepRegistry GetStepRegistry()
        {
            var infos = GetMethods(LibType.Step);
            var registry = new StepRegistry();
            foreach (var info in infos)
            {
                var stepTexts = info.GetCustomAttributes(GetLibType(LibType.Step))
                    .SelectMany(x => x.GetType().GetProperty("Names").GetValue(x, null) as string[]);
                foreach (var stepText in stepTexts)
                {
                    var stepValue = GetStepValue(stepTexts.FirstOrDefault());
                    var isAlias = stepTexts.Count() > 1;
                    var stepMethod = new GaugeMethod
                    {
                        Name = info.FullyQuallifiedName(),
                        ParameterCount = info.GetParameters().Length,
                        StepText = stepText,
                        IsAlias = isAlias,
                        Aliases = stepTexts,
                        MethodInfo = info,
                        ContinueOnFailure = info.IsRecoverableStep(this),
                        StepValue = stepValue
                    };
                    registry.AddStep(stepValue, stepMethod);
                }
            }

            return registry;
        }

        private static string GetStepValue(string stepText)
        {
            return Regex.Replace(stepText, @"(<.*?>)", @"{}");
        }

        private void ScanAndLoad(string path)
        {
            var logger = LogManager.GetLogger("AssemblyLoader");
            logger.Debug("Loading assembly from : {0}", path);
            var assembly = _assemblyWrapper.LoadFrom(path);

            var isReferencingGaugeLib = assembly.GetReferencedAssemblies()
                .Select(name => name.Name)
                .Contains(GaugeLibAssembleName);

            if (!isReferencingGaugeLib)
                return;

            AssembliesReferencingGaugeLib.Add(assembly);


            try
            {
                if (ScreengrabberType is null)
                    ScanForCustomScreengrabber(assembly.GetTypes());

                if (ClassInstanceManagerType is null)
                    ScanForCustomInstanceManager(assembly.GetTypes());
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var e in ex.LoaderExceptions)
                    logger.Error(e);
            }
        }

        private void ScanForCustomScreengrabber(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.IScreenGrabber"
                                              || t.FullName == "Gauge.CSharp.Lib.ICustomScreenshotGrabber"));
            ScreengrabberType = implementingTypes.FirstOrDefault();
        }

        private void ScanForCustomInstanceManager(IEnumerable<Type> types)
        {
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.IClassInstanceManager"));
            ClassInstanceManagerType = implementingTypes.FirstOrDefault();
        }

        private void SetDefaultTypes()
        {
            ClassInstanceManagerType = ClassInstanceManagerType ??
                                       _targetLibAssembly.GetType(LibType.DefaultClassInstanceManager.FullName());
            ScreengrabberType =
                ScreengrabberType ?? _targetLibAssembly.GetType(LibType.DefaultScreenGrabber.FullName());
        }

        private void LoadTargetLibAssembly()
        {
            _targetLibAssembly = _assemblyWrapper.GetCurrentDomainAssemblies()
                .First(x => string.CompareOrdinal(x.GetName().Name, GaugeLibAssembleName) == 0);
        }
    }
}