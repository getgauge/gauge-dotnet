/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Models;

namespace Gauge.Dotnet.Registries;

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
            {LibType.AfterStep, new HashSet<IHookMethod>()},
            {LibType.BeforeConcept, new HashSet<IHookMethod>()},
            {LibType.AfterConcept, new HashSet<IHookMethod>()}
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

    public HashSet<IHookMethod> BeforeConceptHooks => _hooks[LibType.BeforeConcept];

    public HashSet<IHookMethod> AfterConceptHooks => _hooks[LibType.AfterConcept];

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