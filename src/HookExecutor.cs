/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet
{
    public class HookExecutor : MethodExecutor, IHookExecutor
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private IHookRegistry _registry;
        private IExecutionInfoMapper _executionInfoMapper;

        public HookExecutor(IAssemblyLoader assemblyLoader, IReflectionWrapper reflectionWrapper,
            object classInstanceManager, IExecutionInfoMapper mapper) :
            base(assemblyLoader.ClassInstanceManagerType, reflectionWrapper, classInstanceManager)
        {
            _assemblyLoader = assemblyLoader;
            _registry = new HookRegistry(assemblyLoader);
            _executionInfoMapper = mapper;
        }

        public async Task<ExecutionResult> Execute(string hookType, IHooksStrategy strategy, IList<string> applicableTags,
            ExecutionInfo info)
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
                    var context = _executionInfoMapper.ExecutionContextFrom(info);
                    await ExecuteHook(methodInfo, context);
                }
                catch (Exception ex)
                {
                    Logger.Debug($"{hookType} Hook execution failed : {methodInfo.DeclaringType.FullName}.{methodInfo.Name}");
                    var innerException = ex.InnerException ?? ex;
                    executionResult.ExceptionMessage = innerException.Message;
                    executionResult.StackTrace = innerException.StackTrace;
                    executionResult.Source = innerException.Source;
                    executionResult.Success = false;
                }
            }

            return executionResult;
        }

        private async Task ExecuteHook(MethodInfo method, params object[] objects)
        {
            if (HasArguments(method, objects))
                await Execute(method, objects);
            else
                await Execute(method);
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