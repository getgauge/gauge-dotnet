/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Gauge.Dotnet
{
    public class GaugeLoadContext: AssemblyLoadContext, IGaugeLoadContext
    {
        private const string GaugeLibAssemblyName = "Gauge.CSharp.Lib";
        private AssemblyDependencyResolver _resolver;

        public GaugeLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected GaugeLoadContext(string pluginPath, bool collectible) : base(collectible)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        public IEnumerable<Assembly> GetAssembliesReferencingGaugeLib()
        {
            return this.Assemblies.Where(a => a.GetReferencedAssemblies().Any(a => a.Name == GaugeLibAssemblyName));
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            Logger.Debug($"Try load {assemblyName.Name}");
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }
    }

    public class DisposableGaugeLoadContext : GaugeLoadContext, System.IDisposable
    {
        public DisposableGaugeLoadContext(string pluginPath) : base(pluginPath, true)
        {
        }

        public void Dispose()
        {
            Logger.Debug("Unloading GaugeLoadContext, assemblies should be freed up.");
            base.Unload();
        }

    }
}