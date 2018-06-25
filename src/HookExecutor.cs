// Copyright 2018 ThoughtWorks, Inc.
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
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using NLog;

namespace Gauge.Dotnet
{
    public class HookExecutor : MethodExecutor, IHookExecutor
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private IHookRegistry _registry;

        public HookExecutor(IAssemblyLoader assemblyLoader, IReflectionWrapper reflectionWrapper,
            object claasInstanceManager) :
            base(assemblyLoader.ClassInstanceManagerType, reflectionWrapper, claasInstanceManager)
        {
            _assemblyLoader = assemblyLoader;
            _registry = new HookRegistry(assemblyLoader);
        }

        public ExecutionResult ExecuteHooks(string hookType, IHooksStrategy strategy, IList<string> applicableTags,
            ExecutionContext context)
        {
            var methods = GetHookMethods(hookType, strategy, applicableTags);
            var executionResult = new ExecutionResult
            {
                Success = true
            };
            foreach (var method in methods)
            {
                var methodInfo = _registry.MethodFor(method);
                try
                {
                    ExecuteHook(methodInfo, context);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("HookExecutor").Debug("{0} Hook execution failed : {1}.{2}", hookType,
                        methodInfo.DeclaringType.FullName, methodInfo.Name);
                    var innerException = ex.InnerException ?? ex;
                    executionResult.ExceptionMessage = innerException.Message;
                    executionResult.StackTrace = innerException.StackTrace;
                    executionResult.Source = innerException.Source;
                    executionResult.Success = false;
                }
            }

            return executionResult;
        }

        private void ExecuteHook(MethodInfo method, params object[] objects)
        {
            if (HasArguments(method, objects))
                Execute(method, objects);
            else
                Execute(method);
        }


        private static bool HasArguments(MethodInfo method, object[] args)
        {
            if (method.GetParameters().Length != args.Length)
                return false;
            return !args.Where((t, i) => t.GetType() != method.GetParameters()[i].ParameterType).Any();
        }


        private IEnumerable<string> GetHookMethods(string hookType, IHooksStrategy strategy,
            IEnumerable<string> applicableTags)
        {
            var hooksFromRegistry = GetHooksFromRegistry(hookType);
            return strategy.GetApplicableHooks(applicableTags, hooksFromRegistry);
        }


        private IEnumerable<IHookMethod> GetHooksFromRegistry(string hookType)
        {
            _registry = _registry ?? new HookRegistry(_assemblyLoader);
            switch (hookType)
            {
                case "BeforeSuite":
                    return _registry.BeforeSuiteHooks;
                case "BeforeSpec":
                    return _registry.BeforeSpecHooks;
                case "BeforeScenario":
                    return _registry.BeforeScenarioHooks;
                case "BeforeStep":
                    return _registry.BeforeStepHooks;
                case "AfterStep":
                    return _registry.AfterStepHooks;
                case "AfterScenario":
                    return _registry.AfterScenarioHooks;
                case "AfterSpec":
                    return _registry.AfterSpecHooks;
                case "AfterSuite":
                    return _registry.AfterSuiteHooks;
                default:
                    return null;
            }
        }
    }
}