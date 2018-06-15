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
using Gauge.Dotnet.Extensions;

namespace Gauge.Dotnet.Models
{
    [Serializable]
    public class HookRegistry : IHookRegistry
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IDictionary<LibType, HashSet<IHookMethod>> _hooks;

        private readonly IDictionary<string, MethodInfo> _methodMap = new Dictionary<string, MethodInfo>();

        public HookRegistry(IAssemblyLoader assemblyLoader)
        {
            _assemblyLoader = assemblyLoader;
            _hooks = new Dictionary<LibType, HashSet<IHookMethod>>
            {
                {LibType.BeforeSuite, new HashSet<IHookMethod>()},
                {LibType.AfterSuite, new HashSet<IHookMethod>()},
                {LibType.BeforeSpec, new HashSet<IHookMethod>()},
                {LibType.AfterSpec, new HashSet<IHookMethod>()},
                {LibType.BeforeScenario, new HashSet<IHookMethod>()},
                {LibType.AfterScenario, new HashSet<IHookMethod>()},
                {LibType.BeforeStep, new HashSet<IHookMethod>()},
                {LibType.AfterStep, new HashSet<IHookMethod>()}
            };

            foreach (var type in _hooks.Keys)
                AddHookOfType(type, assemblyLoader.GetMethods(type));
        }

        public HashSet<IHookMethod> BeforeSuiteHooks => _hooks[LibType.BeforeSuite];

        public HashSet<IHookMethod> AfterSuiteHooks => _hooks[LibType.AfterSuite];

        public HashSet<IHookMethod> BeforeSpecHooks => _hooks[LibType.BeforeSpec];

        public HashSet<IHookMethod> AfterSpecHooks => _hooks[LibType.AfterSpec];

        public HashSet<IHookMethod> BeforeScenarioHooks => _hooks[LibType.BeforeScenario];

        public HashSet<IHookMethod> AfterScenarioHooks => _hooks[LibType.AfterScenario];

        public HashSet<IHookMethod> BeforeStepHooks => _hooks[LibType.BeforeStep];

        public HashSet<IHookMethod> AfterStepHooks => _hooks[LibType.AfterStep];

        public MethodInfo MethodFor(string method)
        {
            return _methodMap[method];
        }

        private void AddHookOfType(LibType hookType, IEnumerable<MethodInfo> hooks)
        {
            foreach (var methodInfo in hooks)
            {
                var fullyQuallifiedName = methodInfo.FullyQuallifiedName();
                if (!_methodMap.ContainsKey(fullyQuallifiedName))
                    _methodMap.Add(fullyQuallifiedName, methodInfo);
            }

            _hooks[hookType].UnionWith(hooks.Select(info => new HookMethod(hookType, info, _assemblyLoader)));
        }
    }
}