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
    public class GaugeLoadContext: AssemblyLoadContext, IGaugeLoadContext, System.IDisposable
    {
        private const string GaugeLibAssemblyName = "Gauge.CSharp.Lib";
        private AssemblyDependencyResolver _resolver;

        public GaugeLoadContext(string pluginPath, bool collectibe = false) : base(collectibe)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        public void Dispose()
        {
            if (IsCollectible)
            {
                Logger.Debug("Unloading GaugeLoadContext, assemblies should be freed up.");
                base.Unload();
            }
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
}