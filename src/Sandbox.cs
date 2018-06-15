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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Converters;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using NLog;

namespace Gauge.Dotnet
{
    [Serializable]
    public class Sandbox : ISandbox
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IActivatorWrapper activatorWrapper;
        private readonly Type instanceManagerType;
        private readonly IReflectionWrapper reflectionWrapper;

        private object _classInstanceManager;

        private IHookRegistry _hookRegistry;

        public Sandbox(IAssemblyLoader assemblyLoader, IHookRegistry hookRegistry, IActivatorWrapper activatorWrapper,
            IReflectionWrapper typeWrapper)
        {
            LogConfiguration.Initialize();
            _assemblyLoader = assemblyLoader;
            _hookRegistry = hookRegistry;
            this.activatorWrapper = activatorWrapper;
            reflectionWrapper = typeWrapper;
            instanceManagerType = _assemblyLoader.ClassInstanceManagerType;
            LoadClassInstanceManager();
        }

        private IDictionary<string, MethodInfo> MethodMap { get; set; }

        [DebuggerStepperBoundary]
        [DebuggerHidden]
        public ExecutionResult ExecuteMethod(GaugeMethod gaugeMethod, params string[] args)
        {
            var method = gaugeMethod.MethodInfo;
            var executionResult = new ExecutionResult {Success = true};
            var logger = LogManager.GetLogger("Sandbox");
            try
            {
                var parameters = args.Select(o =>
                {
                    try
                    {
                        return GetTable(o);
                    }
                    catch
                    {
                        return o;
                    }
                }).ToArray();
                logger.Debug("Executing method: {0}", gaugeMethod.Name);
                Execute(method, StringParamConverter.TryConvertParams(method, parameters));
            }
            catch (Exception ex)
            {
                logger.Debug("Error executing {0}", method.Name);
                var innerException = ex.InnerException ?? ex;
                executionResult.ExceptionMessage = innerException.Message;
                executionResult.StackTrace = innerException is AggregateException
                    ? innerException.ToString()
                    : innerException.StackTrace;
                executionResult.Source = innerException.Source;
                executionResult.Success = false;
                executionResult.Recoverable = gaugeMethod.ContinueOnFailure;
            }

            return executionResult;
        }

        public List<GaugeMethod> GetStepMethods()
        {
            var infos = _assemblyLoader.GetMethods(LibType.Step);
            MethodMap = new Dictionary<string, MethodInfo>();
            foreach (var info in infos)
            {
                var methodId = info.FullyQuallifiedName();
                MethodMap.Add(methodId, info);
                LogManager.GetLogger("Sandbox").Debug("Scanned and caching Gauge Step: {0}, Recoverable: {1}", methodId,
                    info.IsRecoverableStep(_assemblyLoader));
            }

            return MethodMap.Keys.Select(s =>
            {
                var method = MethodMap[s];
                return new GaugeMethod
                {
                    MethodInfo = MethodMap[s],
                    Name = s,
                    ParameterCount = method.GetParameters().Length,
                    ContinueOnFailure = method.IsRecoverableStep(_assemblyLoader)
                };
            }).ToList();
        }

        public List<string> GetAllStepTexts()
        {
            return GetStepMethods().SelectMany(GetStepTexts).ToList();
        }

        public IEnumerable<string> GetStepTexts(GaugeMethod gaugeMethod)
        {
            var stepMethod = MethodMap[gaugeMethod.Name];
            return stepMethod.GetCustomAttributes(_assemblyLoader.GetLibType(LibType.Step))
                .SelectMany(x => x.GetType().GetProperty("Names").GetValue(x, null) as string[]);
        }

        public bool TryScreenCapture(out byte[] screenShotBytes)
        {
            try
            {
                var instance = activatorWrapper.CreateInstance(_assemblyLoader.ScreengrabberType);
                if (instance != null)
                {
                    screenShotBytes =
                        reflectionWrapper.InvokeMethod(_assemblyLoader.ScreengrabberType, instance, "TakeScreenShot") as
                            byte[];
                    return true;
                }
            }
            catch
            {
                //do nothing, return
            }

            screenShotBytes = null;
            return false;
        }

        public void ClearObjectCache()
        {
            reflectionWrapper.InvokeMethod(instanceManagerType, _classInstanceManager, "ClearCache");
        }

        public void StartExecutionScope(string tag)
        {
            reflectionWrapper.InvokeMethod(instanceManagerType, _classInstanceManager, "StartScope", tag);
        }

        public void CloseExectionScope()
        {
            reflectionWrapper.InvokeMethod(instanceManagerType, _classInstanceManager, "CloseScope");
        }

        [DebuggerStepperBoundary]
        [DebuggerHidden]
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
                var methodInfo = _hookRegistry.MethodFor(method);
                try
                {
                    ExecuteHook(methodInfo, context);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("Sandbox").Debug("{0} Hook execution failed : {1}.{2}", hookType,
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


        private object GetTable(string jsonString)
        {
            var serializer = new DataContractJsonSerializer(_assemblyLoader.GetLibType(LibType.Table));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return serializer.ReadObject(ms);
            }
        }

        [DebuggerHidden]
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
            for (var i = 0; i < args.Length; i++)
                if (args[i].GetType() != method.GetParameters()[i].ParameterType)
                    return false;
            return true;
        }

        private IEnumerable<string> GetHookMethods(string hookType, IHooksStrategy strategy,
            IEnumerable<string> applicableTags)
        {
            var hooksFromRegistry = GetHooksFromRegistry(hookType);
            return strategy.GetApplicableHooks(applicableTags, hooksFromRegistry);
        }

        private IEnumerable<IHookMethod> GetHooksFromRegistry(string hookType)
        {
            _hookRegistry = _hookRegistry ?? new HookRegistry(_assemblyLoader);
            switch (hookType)
            {
                case "BeforeSuite":
                    return _hookRegistry.BeforeSuiteHooks;
                case "BeforeSpec":
                    return _hookRegistry.BeforeSpecHooks;
                case "BeforeScenario":
                    return _hookRegistry.BeforeScenarioHooks;
                case "BeforeStep":
                    return _hookRegistry.BeforeStepHooks;
                case "AfterStep":
                    return _hookRegistry.AfterStepHooks;
                case "AfterScenario":
                    return _hookRegistry.AfterScenarioHooks;
                case "AfterSpec":
                    return _hookRegistry.AfterSpecHooks;
                case "AfterSuite":
                    return _hookRegistry.AfterSuiteHooks;
                default:
                    return null;
            }
        }

        private void Execute(MethodInfo method, params object[] parameters)
        {
            var typeToLoad = method.DeclaringType;
            var instance =
                reflectionWrapper.InvokeMethod(instanceManagerType, _classInstanceManager, "Get", typeToLoad);
            var logger = LogManager.GetLogger("Sandbox");
            if (instance == null)
            {
                var error = "Could not load instance type: " + typeToLoad;
                logger.Error(error);
                throw new Exception(error);
            }

            reflectionWrapper.Invoke(method, instance, parameters);
        }

        private void LoadClassInstanceManager()
        {
            if (instanceManagerType != null)
            {
                var logger = LogManager.GetLogger("Sandbox");
                _classInstanceManager = activatorWrapper.CreateInstance(instanceManagerType);
                logger.Debug("Loaded Instance Manager of Type:" + _classInstanceManager.GetType().FullName);
                reflectionWrapper.InvokeMethod(instanceManagerType, _classInstanceManager, "Initialize",
                    _assemblyLoader.AssembliesReferencingGaugeLib);
            }
        }
    }
}