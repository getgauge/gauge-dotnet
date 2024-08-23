/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System.Reflection;

namespace Gauge.Dotnet;

/* NOTE: LockFreeGaugeLoadContext is required because GaugeLoadContext uses 
// AssemblyLoadContext.LoadFromAssemblyPath which holds a filesystem lock 
// on the assembly file. This causes Run/Debug to fail because these
// actions run as separate process and cannot write to build output dir.
// GaugeLoadContext is also required because certain assemblies are shipped
// with runtime specific artifacts, and loading raw bytes is risky, can 
// cause BadImageFormatException at runtime. */
public class LockFreeGaugeLoadContext : GaugeLoadContext
{
    public LockFreeGaugeLoadContext(IAssemblyLocater locater)
        : base(locater)
    {
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        Logger.Debug($"Try load {assemblyName.Name} in LockFreeGaugeLoadContext");
        if (assemblyPath != null)
        {
            using (var fileStream = System.IO.File.OpenRead(assemblyPath))
            {
                return LoadFromStream(fileStream);
            }
        }
        return null;
    }
}