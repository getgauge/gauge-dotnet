/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Messages;

namespace Gauge.Dotnet.Executors;

public class HookExecutor : MethodExecutor, IHookExecutor
{
    private readonly IAssemblyLoader _assemblyLoader;
    private readonly IHookRegistry _registry;
    private readonly IExecutionInfoMapper _executionInfoMapper;

    public HookExecutor(IAssemblyLoader assemblyLoader, IExecutionInfoMapper mapper, IHookRegistry registry, ILogger<HookExecutor> logger)
        : base(assemblyLoader, logger)
    {
        _assemblyLoader = assemblyLoader;
        _registry = registry;
        _executionInfoMapper = mapper;
    }

    public async Task<ExecutionResult> Execute(string hookType, IHooksStrategy strategy, IList<string> applicableTags, int streamId, ExecutionInfo info)
    {
        var methods = GetHookMethods(hookType, strategy, applicableTags);
        var executionResult = new ExecutionResult
        {
            Success = true,
            SkipScenario = false
        };
        foreach (var method in methods)
        {
            var methodInfo = _registry.MethodFor(method);
            try
            {
                var context = _executionInfoMapper.ExecutionContextFrom(info, streamId);
                await ExecuteHook(methodInfo, context, context);
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();
                if (baseException != null &&
                    baseException.GetType().Name.Contains("SkipScenario", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogDebug("Skipping scenario when executing hook: {ClassFullName}.{MethodName} : {ExceptionMessage}", methodInfo.DeclaringType.FullName, methodInfo.Name, baseException.Message);
                    executionResult.StackTrace = baseException.StackTrace;
                    executionResult.ExceptionMessage = baseException.Message;
                    executionResult.Source = baseException.Source;
                    executionResult.Success = true;
                    executionResult.SkipScenario = true;
                }
                else
                {
                    Logger.LogDebug("{HookType} Hook execution failed : {ClassFullName}.{MethodName}", hookType, methodInfo.DeclaringType.FullName, methodInfo.Name);
                    var innerException = ex.InnerException ?? ex;
                    executionResult.ExceptionMessage = innerException.Message;
                    executionResult.StackTrace = innerException.StackTrace;
                    executionResult.Source = innerException.Source;
                    executionResult.Success = false;
                }
            }
        }

        return executionResult;
    }

    private async Task ExecuteHook(MethodInfo method, object context, params object[] objects)
    {
        if (HasArguments(method, objects))
            await Execute(method, context, objects);
        else
            await Execute(method, context);
    }


    private static bool HasArguments(MethodInfo method, object[] args)
    {
        if (method.GetParameters().Length != args.Length)
            return false;
        return !args.Where((t, i) => t.GetType() != method.GetParameters()[i].ParameterType).Any();
    }


    private IEnumerable<string> GetHookMethods(string hookType, IHooksStrategy strategy, IEnumerable<string> applicableTags)
    {
        var hooksFromRegistry = GetHooksFromRegistry(hookType);
        return strategy.GetApplicableHooks(applicableTags, hooksFromRegistry);
    }


    private IEnumerable<IHookMethod> GetHooksFromRegistry(string hookType)
    {
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
            case "BeforeConcept":
                return _registry.BeforeConceptHooks;
            case "AfterConcept":
                return _registry.AfterConceptHooks;
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