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

using System.Collections.Generic;
using System.Diagnostics;
using Gauge.CSharp.Core;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Google.Protobuf;
using NLog;

namespace Gauge.Dotnet
{
    public class ExecutionHelper : IExecutionHelper
    {
        private readonly IActivatorWrapper _activatorWrapper;
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IHookExecutor _hookExecutor;
        private readonly IReflectionWrapper _reflectionWrapper;
        private readonly IStepExecutor _stepExecutor;

        private object _classInstanceManager;

        public ExecutionHelper(IReflectionWrapper reflectionWrapper, IAssemblyLoader assemblyLoader,
            IActivatorWrapper activatorWrapper, IHookExecutor hookExecutor, IStepExecutor stepExecutor)
        {
            _reflectionWrapper = reflectionWrapper;
            _assemblyLoader = assemblyLoader;
            _activatorWrapper = activatorWrapper;
            LoadClassInstanceManager();
            hookExecutor.SetClassInstanceManager(_classInstanceManager);
            stepExecutor.SetClassInstanceManager(_classInstanceManager);
            _hookExecutor = hookExecutor;
            _stepExecutor = stepExecutor;
        }


        [DebuggerHidden]
        public ProtoExecutionResult ExecuteStep(GaugeMethod method, params string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            var builder = new ProtoExecutionResult
            {
                Failed = false
            };
            var executionResult = _stepExecutor.ExecuteStep(method, args);

            builder.ExecutionTime = stopwatch.ElapsedMilliseconds;
            if (executionResult.Success) return builder;
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            builder.Failed = true;
            var isScreenShotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
            if (isScreenShotEnabled == null || isScreenShotEnabled.ToLower() != "false")
                builder.ScreenShot = TakeScreenshot();
            builder.ErrorMessage = executionResult.ExceptionMessage;
            builder.StackTrace = executionResult.StackTrace;
            builder.RecoverableError = executionResult.Recoverable;
            builder.ExecutionTime = elapsedMilliseconds;

            return builder;
        }

        public void ClearCache()
        {
            _reflectionWrapper.InvokeMethod(_assemblyLoader.ClassInstanceManagerType, _classInstanceManager,
                "ClearCache");
        }

        public void StartExecutionScope(string tag)
        {
            _reflectionWrapper.InvokeMethod(_assemblyLoader.ClassInstanceManagerType, _classInstanceManager,
                "StartScope", tag);
        }

        public void CloseExectionScope()
        {
            _reflectionWrapper.InvokeMethod(_assemblyLoader.ClassInstanceManagerType, _classInstanceManager,
                "CloseScope");
        }

        [DebuggerHidden]
        public ProtoExecutionResult ExecuteHooks(string hookType, HooksStrategy strategy, IList<string> applicableTags,
            ExecutionContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ProtoExecutionResult
            {
                Failed = false
            };

            var executionResult = _hookExecutor.ExecuteHooks(hookType, strategy, applicableTags, context);

            result.ExecutionTime = stopwatch.ElapsedMilliseconds;
            if (executionResult.Success) return result;
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            result.Failed = true;
            var isScreenShotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
            if (isScreenShotEnabled == null || isScreenShotEnabled.ToLower() != "false")
                result.ScreenShot = TakeScreenshot();
            result.ErrorMessage = executionResult.ExceptionMessage;
            result.StackTrace = executionResult.StackTrace;
            result.RecoverableError = executionResult.Recoverable;
            result.ExecutionTime = elapsedMilliseconds;

            return result;
        }

        private ByteString TakeScreenshot()
        {
            return TryScreenCapture(out var screenShotBytes)
                ? ByteString.CopyFrom(screenShotBytes)
                : ByteString.Empty;
        }

        private bool TryScreenCapture(out byte[] screenShotBytes)
        {
            try
            {
                var instance = _activatorWrapper.CreateInstance(_assemblyLoader.ScreengrabberType);
                if (instance != null)
                {
                    screenShotBytes =
                        _reflectionWrapper.InvokeMethod(_assemblyLoader.ScreengrabberType, instance,
                                "TakeScreenShot") as
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

        private void LoadClassInstanceManager()
        {
            var instanceManagerType = _assemblyLoader.ClassInstanceManagerType;
            if (instanceManagerType == null) return;
            var logger = LogManager.GetLogger("ExecutionHelper");
            _classInstanceManager = _activatorWrapper.CreateInstance(instanceManagerType);
            logger.Debug("Loaded Instance Manager of Type:" + _classInstanceManager.GetType().FullName);
            _reflectionWrapper.InvokeMethod(instanceManagerType, _classInstanceManager, "Initialize",
                _assemblyLoader.AssembliesReferencingGaugeLib);
        }
    }
}