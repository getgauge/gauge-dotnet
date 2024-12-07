/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System.Reflection;
using System.Runtime.Loader;

namespace Gauge.Dotnet.Loaders;

public class GaugeLoadContext : AssemblyLoadContext, IGaugeLoadContext
{
    protected readonly ILogger _logger;
    protected AssemblyDependencyResolver _resolver;
    private List<Assembly> _assembliesReferencingGaugeLib;

    public GaugeLoadContext(IAssemblyLocater assemblyLocater, ILogger logger)
    {
        _logger = logger;
        var assemblyPath = assemblyLocater.GetTestAssembly();
        _logger.LogDebug("Loading assembly from : {AssemblyPath}", assemblyPath);
        _resolver = new AssemblyDependencyResolver(assemblyPath);
    }

    public IEnumerable<Assembly> GetLoadedAssembliesReferencingGaugeLib()
    {
        return _assembliesReferencingGaugeLib ??= Assemblies.Where(a => a.GetReferencedAssemblies().Any(a => a.Name == GaugeLibAssemblyName)).ToList();
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        _logger.LogDebug("Try load {AssemblyName} in GaugeLoadContext", assemblyName.Name);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }
        return null;
    }
}