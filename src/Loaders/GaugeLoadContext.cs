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
    private const string GaugeLibAssemblyName = "Gauge.CSharp.Lib";
    protected readonly ILogger _logger;
    protected AssemblyDependencyResolver _resolver;

    public GaugeLoadContext(IAssemblyLocater locater, ILogger logger)
    {
        _resolver = new AssemblyDependencyResolver(locater.GetTestAssembly());
        _logger = logger;
    }

    public IEnumerable<Assembly> GetAssembliesReferencingGaugeLib()
    {
        return Assemblies.Where(a => a.GetReferencedAssemblies().Any(a => a.Name == GaugeLibAssemblyName));
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