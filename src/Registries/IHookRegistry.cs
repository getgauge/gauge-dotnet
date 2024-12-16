/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.Dotnet.Models;

namespace Gauge.Dotnet.Registries
{
    public interface IHookRegistry
    {
        HashSet<IHookMethod> BeforeSuiteHooks { get; }
        HashSet<IHookMethod> AfterSuiteHooks { get; }
        HashSet<IHookMethod> BeforeSpecHooks { get; }
        HashSet<IHookMethod> AfterSpecHooks { get; }
        HashSet<IHookMethod> BeforeScenarioHooks { get; }
        HashSet<IHookMethod> AfterScenarioHooks { get; }
        HashSet<IHookMethod> BeforeStepHooks { get; }
        HashSet<IHookMethod> AfterStepHooks { get; }
        HashSet<IHookMethod> BeforeConceptHooks { get; }
        HashSet<IHookMethod> AfterConceptHooks { get; }
        MethodInfo MethodFor(string method);
    }
}