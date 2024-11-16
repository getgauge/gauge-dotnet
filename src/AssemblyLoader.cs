﻿/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using System.Text.RegularExpressions;
using Gauge.Dotnet.Exceptions;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;

namespace Gauge.Dotnet;

public class AssemblyLoader : IAssemblyLoader
{
    private const string GaugeLibAssemblyName = "Gauge.CSharp.Lib";
    private readonly IReflectionWrapper _reflectionWrapper;
    private readonly IGaugeLoadContext _gaugeLoadContext;
    private Assembly _targetLibAssembly;
    private object _classInstanceManager;

    private readonly IActivatorWrapper _activatorWrapper;
    private readonly IStepRegistry _registry;
    private readonly ILogger<AssemblyLoader> _logger;

    public AssemblyLoader(IAssemblyLocater locater, IGaugeLoadContext gaugeLoadContext, IReflectionWrapper reflectionWrapper,
        IActivatorWrapper activatorWrapper, IStepRegistry registry, ILogger<AssemblyLoader> logger)
    {
        var assemblyPath = locater.GetTestAssembly();
        _reflectionWrapper = reflectionWrapper;
        _activatorWrapper = activatorWrapper;
        AssembliesReferencingGaugeLib = new List<Assembly>();
        _registry = registry;
        _logger = logger;

        _logger.LogDebug("Loading assembly from : {AssemblyPath}", assemblyPath);
        _gaugeLoadContext = gaugeLoadContext;
        _targetLibAssembly = _gaugeLoadContext.LoadFromAssemblyName(new AssemblyName(GaugeLibAssemblyName));
        VerifyTargetAssemblyVersion();
        ScanAndLoad(assemblyPath);
        AssembliesReferencingGaugeLib = _gaugeLoadContext.GetAssembliesReferencingGaugeLib().ToList();
        _logger.LogDebug("Number of AssembliesReferencingGaugeLib : {AssembliesReferencingGaugeLibCount}", AssembliesReferencingGaugeLib.Count);
        SetDefaultTypes();
        _registry = GetStepRegistry();
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
        try
        {
            return _targetLibAssembly.ExportedTypes.First(t => t.FullName == type.FullName());
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Cannot locate {type.FullName()} in Gauge.CSharp.Lib", ex);
        }
    }


    public IStepRegistry GetStepRegistry()
    {
        _logger.LogDebug("Building StepRegistry...");
        var infos = GetMethods(LibType.Step);
        _logger.LogDebug("{InfosCount} Step implementations found. Adding to registry...", infos.Count());
        foreach (var info in infos)
        {
            var stepTexts = Attribute.GetCustomAttributes(info).Where(x => x.GetType().FullName == LibType.Step.FullName())
                .SelectMany(x => x.GetType().GetProperty("Names").GetValue(x, null) as string[]);
            foreach (var stepText in stepTexts)
            {
                var stepValue = GetStepValue(stepText);
                if (_registry.ContainsStep(stepValue))
                {
                    _logger.LogDebug("'{StepValue}': implementation found in StepRegistry, setting reflected methodInfo", stepValue);
                    _registry.MethodFor(stepValue).MethodInfo = info;
                    _registry.MethodFor(stepValue).ContinueOnFailure = info.IsRecoverableStep(this);
                }
                else
                {
                    _logger.LogDebug("'{StepValue}': no implementation in StepRegistry, adding via reflection", stepValue);
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
        if (_classInstanceManager != null) return _classInstanceManager;
        if (ClassInstanceManagerType == null) return null;
        _classInstanceManager = _activatorWrapper.CreateInstance(ClassInstanceManagerType);
        _logger.LogDebug("Loaded Instance Manager of Type: {ClassInstanceManagerType}", _classInstanceManager.GetType().FullName);
        _reflectionWrapper.InvokeMethod(ClassInstanceManagerType, _classInstanceManager, "Initialize", AssembliesReferencingGaugeLib);
        return _classInstanceManager;
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
            var exportedTypes = _gaugeLoadContext.GetAssembliesReferencingGaugeLib().SelectMany(x => x.ExportedTypes).ToList();
            if (ScreenshotWriter is null)
                ScanForCustomScreenshotWriter(exportedTypes);

            if (ClassInstanceManagerType is null)
                ScanForCustomInstanceManager(exportedTypes);
        }
        catch (ReflectionTypeLoadException ex)
        {
            foreach (var e in ex.LoaderExceptions)
                _logger.LogError(e.ToString());
        }
    }

    private void ScanForCustomScreenshotWriter(IEnumerable<Type> types)
    {
        var implementingTypes = types.Where(type => type.GetInterfaces().Any(t => t.FullName == "Gauge.CSharp.Lib.ICustomScreenshotWriter"));
        ScreenshotWriter = implementingTypes.FirstOrDefault();
        if (ScreenshotWriter is null) return;
        var csg = _activatorWrapper.CreateInstance(ScreenshotWriter);
        var gaugeScreenshotsType = _targetLibAssembly.ExportedTypes.First(x => x.FullName == "Gauge.CSharp.Lib.GaugeScreenshots");
        _reflectionWrapper.InvokeMethod(gaugeScreenshotsType, null, "RegisterCustomScreenshotWriter",
            BindingFlags.Static | BindingFlags.Public, new[] { csg });
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

    private void VerifyTargetAssemblyVersion()
    {
        var myLibAssemblyVersion = Assembly.GetEntryAssembly().GetReferencedAssemblies().FirstOrDefault(x => x.Name == GaugeLibAssemblyName).Version;
        var targetAssemblyVersion = _targetLibAssembly.GetName().Version;
        if (myLibAssemblyVersion.Major != targetAssemblyVersion.Major
            || myLibAssemblyVersion.Minor != targetAssemblyVersion.Minor)
        {
            throw new GaugeLibVersionMismatchException(myLibAssemblyVersion);
        }

    }
}