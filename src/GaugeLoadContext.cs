/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System.Reflection;
using System.Runtime.Loader;

namespace Gauge.Dotnet;

public class GaugeLoadContext : AssemblyLoadContext, IGaugeLoadContext
{
    private const string GaugeLibAssemblyName = "Gauge.CSharp.Lib";
    protected AssemblyDependencyResolver _resolver;

    public GaugeLoadContext(IAssemblyLocater locater)
    {
        _resolver = new AssemblyDependencyResolver(locater.GetTestAssembly());
    }

    public IEnumerable<Assembly> GetAssembliesReferencingGaugeLib()
    {
        return this.Assemblies.Where(a => a.GetReferencedAssemblies().Any(a => a.Name == GaugeLibAssemblyName));
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        Logger.Debug($"Try load {assemblyName.Name} in GaugeLoadContext");
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }
        return null;
    }
}