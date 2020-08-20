/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;

namespace Gauge.Dotnet
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private const string GaugeLibAssemblyName = "Gauge.CSharp.Lib";
        private readonly IReflectionWrapper _reflectionWrapper;
        private readonly IGaugeLoadContext _gaugeLoadContext;
        private Assembly _targetLibAssembly;

        private readonly IActivatorWrapper _activatorWrapper;
        private readonly IStepRegistry _registry;

        public AssemblyLoader(string assemblyPath, IGaugeLoadContext gaugeLoadContext,
            IReflectionWrapper reflectionWrapper, IActivatorWrapper activatorWrapper, IStepRegistry registry)
        {
            _reflectionWrapper = reflectionWrapper;
            _activatorWrapper = activatorWrapper;
            AssembliesReferencingGaugeLib = new List<Assembly>();
            _registry = registry;
            
            Logger.Debug($"Loading assembly from : {assemblyPath}");
            _gaugeLoadContext = gaugeLoadContext;
            this._targetLibAssembly = _gaugeLoadContext.LoadFromAssemblyName(new AssemblyName(GaugeLibAssemblyName));
            ScanAndLoad(assemblyPath);
            AssembliesReferencingGaugeLib = _gaugeLoadContext.GetAssembliesReferencingGaugeLib().ToList();
            Logger.Debug($"Number of AssembliesReferencingGaugeLib : {AssembliesReferencingGaugeLib.Count()}");
            SetDefaultTypes();
        }

        public List<Assembly> AssembliesReferencingGaugeLib { get; }
        public Type ScreenshotWriter { get; private set; }
        public Type ClassInstanceManagerType { get; private set; }

        public IEnumerable<MethodInfo> GetMethods(LibType type)
        {
            var attributeType = _targetLibAssembly.ExportedTypes.First(x => x.FullName == type.FullName());

            IEnumerable<MethodInfo> MethodSelector(Type t)
            {
                return _reflectionWrapper.GetMethods(t)
                    .Where(info => info.GetCustomAttributes(false).Any(attributeType.IsInstanceOfType));
            }
            return AssembliesReferencingGaugeLib.SelectMany(assembly => assembly.ExportedTypes.SelectMany(MethodSelector));
        }

        public Type GetLibType(LibType type)
        {
            return _targetLibAssembly.ExportedTypes.First(t => t.FullName == type.FullName());
        }


        public IStepRegistry GetStepRegistry()
        {
            Logger.Debug("Building StepRegistry...");
            var infos = GetMethods(LibType.Step);
            Logger.Debug($"{infos.Count()} Step implementations found. Adding to registry...");
            foreach (var info in infos)
            {
                var stepTexts = info.GetCustomAttributes().Where(x => x.GetType().FullName == LibType.Step.FullName())
                    .SelectMany(x => x.GetType().GetProperty("Names").GetValue(x, null) as string[]);
                foreach (var stepText in stepTexts)
                {
                    var stepValue = GetStepValue(stepText);
                    if (_registry.ContainsStep(stepValue))
                    {
                        Logger.Debug($"'{stepValue}': implementation found in StepRegistry, setting reflected methodInfo");
                        _registry.MethodFor(stepValue).MethodInfo = info;
                        _registry.MethodFor(stepValue).ContinueOnFailure = info.IsRecoverableStep(this);
                    }
                    else
                    {
                        Logger.Debug($"'{stepValue}': no implementation in StepRegistry, adding via reflection");
                        var hasAlias = stepTexts.Count() > 1;
                        var stepMethod = new GaugeMethod
                        {
                            Name = info.FullyQuallifiedName(),
                            ParameterCount = info.GetParameters().Length,
                            StepText = stepText,
                            HasAlias = hasAlias,
                            Aliases = stepTexts,
                            MethodInfo = info,
                            ContinueOnFailure = info.IsRecoverableStep(this),
                            StepValue = stepValue,
                            IsExternal = true,
                        };
                        _registry.AddStep(stepValue, stepMethod);
                    }

                }
            }
            return _registry;
        }

        public object GetClassInstanceManager()
        {
            if (ClassInstanceManagerType == null) return null;
            var classInstanceManager = _activatorWrapper.CreateInstance(ClassInstanceManagerType);
            Logger.Debug("Loaded Instance Manager of Type:" + classInstanceManager.GetType().FullName);
            _reflectionWrapper.InvokeMethod(ClassInstanceManagerType, classInstanceManager, "Initialize",
                AssembliesReferencingGaugeLib);
            return classInstanceManager;
        }

        private static string GetStepValue(string stepText)
        {
            return Regex.Replace(stepText, @"(<.*?>)", @"{}");
        }

        private void ScanAndLoad(string path)
        {
            var assembly = _gaugeLoadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                _gaugeLoadContext.LoadFromAssemblyName(reference);
            }
            try
            {
                if (ScreenshotWriter is null)
                    ScanForCustomScreenshotWriter(assembly.ExportedTypes);

                if (ClassInstanceManagerType is null)
                    ScanForCustomInstanceManager(assembly.ExportedTypes);
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var e in ex.LoaderExceptions)
                    Logger.Error(e.ToString());
            }
        }

        private void ScanForCustomScreenshotWriter(IEnumerable<Type> types)
        {
            var deprecatedImplementations = types.Where(type => type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.ICustomScreenshotGrabber"));
            if (deprecatedImplementations.Any())
            {
                Logger.Error("These types implement DEPRECATED ICustomScreenshotGrabber interface and will not be used. Use ICustomScreenshotWriter instead.\n" +
                    deprecatedImplementations.Select(x => x.FullName).Aggregate((a, b) => $"{a}, {b}"));
            }
            var implementingTypes = types.Where(type =>
                type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.ICustomScreenshotWriter"));
            ScreenshotWriter = implementingTypes.FirstOrDefault();
            if (ScreenshotWriter is null) return;
            var csg = _activatorWrapper.CreateInstance(ScreenshotWriter);
            var gaugeScreenshotsType = _targetLibAssembly.ExportedTypes.First(x => x.FullName == "Gauge.CSharp.Lib.GaugeScreenshots");
            _reflectionWrapper.InvokeMethod(gaugeScreenshotsType, null, "RegisterCustomScreenshotWriter",
                BindingFlags.Static | BindingFlags.Public, new[] {csg});
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
            ScreenshotWriter = ScreenshotWriter ?? 
                _targetLibAssembly.GetType(LibType.DefaultScreenshotWriter.FullName());
        }
    }
}