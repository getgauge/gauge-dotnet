/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet
{
    public class ExecutionOrchestrator : IExecutionOrchestrator
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly object _classInstanceManager;
        private readonly IHookExecutor _hookExecutor;
        private readonly IReflectionWrapper _reflectionWrapper;
        private readonly IStepExecutor _stepExecutor;

        public ExecutionOrchestrator(IReflectionWrapper reflectionWrapper, IAssemblyLoader assemblyLoader,
            object classInstanceManager, IHookExecutor hookExecutor,
            IStepExecutor stepExecutor)
        {
            _reflectionWrapper = reflectionWrapper;
            _assemblyLoader = assemblyLoader;
            _classInstanceManager = classInstanceManager;
            _hookExecutor = hookExecutor;
            _stepExecutor = stepExecutor;
        }


        [DebuggerHidden]
        public async Task<ProtoExecutionResult> ExecuteStep(GaugeMethod method, params string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            var executionResult = await _stepExecutor.Execute(method, args);
            return BuildResult(stopwatch, executionResult);
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

        public void CloseExecutionScope()
        {
            _reflectionWrapper.InvokeMethod(_assemblyLoader.ClassInstanceManagerType, _classInstanceManager,
                "CloseScope");
        }

        public IEnumerable<string> GetAllPendingMessages()
        {
            var messageCollectorType = _assemblyLoader.GetLibType(LibType.MessageCollector);
            return _reflectionWrapper.InvokeMethod(messageCollectorType, null, "GetAllPendingMessages",
                BindingFlags.Static | BindingFlags.Public) as IEnumerable<string>;
        }

        public IEnumerable<string> GetAllPendingScreenshotFiles()
        {
            var messageCollectorType = _assemblyLoader.GetLibType(LibType.ScreenshotFilesCollector);
            return _reflectionWrapper.InvokeMethod(messageCollectorType, null, "GetAllPendingScreenshotFiles",
                BindingFlags.Static | BindingFlags.Public) as IEnumerable<string>;
        }

        [DebuggerHidden]
        public async Task<ProtoExecutionResult> ExecuteHooks(string hookType, HooksStrategy strategy,
            IList<string> applicableTags,
            ExecutionInfo info)
        {
            var stopwatch = Stopwatch.StartNew();
            var executionResult = await _hookExecutor.Execute(hookType, strategy, applicableTags, info);
            return BuildResult(stopwatch, executionResult);
        }

        private ProtoExecutionResult BuildResult(Stopwatch stopwatch, ExecutionResult executionResult)
        {
            var result = new ProtoExecutionResult
            {
                Failed = false,
                ExecutionTime = stopwatch.ElapsedMilliseconds
            };
            var allPendingMessages = GetAllPendingMessages().Where(m => m != null);
            result.Message.AddRange(allPendingMessages);
            var allPendingScreenShotFiles = GetAllPendingScreenshotFiles().Where(s => s != null);
            result.ScreenshotFiles.AddRange(allPendingScreenShotFiles);
            if (executionResult.Success) return result;
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            result.Failed = true;
            var isScreenShotEnabled = Utils.TryReadEnvValue("SCREENSHOT_ON_FAILURE");
            if (isScreenShotEnabled == null || isScreenShotEnabled.ToLower() != "false")
            {
                var screenshotFile = TryScreenCapture();
                if(!string.IsNullOrEmpty(screenshotFile)){
                    result.FailureScreenshotFile = screenshotFile;
                }
            }

            result.ErrorMessage = executionResult.ExceptionMessage;
            if (!string.IsNullOrEmpty(executionResult.StackTrace))
            {
                result.StackTrace = executionResult.StackTrace;
            }
            result.RecoverableError = executionResult.Recoverable;
            result.ExecutionTime = elapsedMilliseconds;
            return result;
        }

        private string TryScreenCapture()
        {
            try
            {
                InvokeScreenshotCapture();
            }
            catch (System.Exception ex)
            {
                Logger.Warning($"Unable to capture screenshot, CustomScreenshotWriter is probably not set.({ex.Message})\n{ex.StackTrace}");
                return null;
            }
            var messageCollectorType = _assemblyLoader.GetLibType(LibType.ScreenshotFilesCollector);
            return (_reflectionWrapper.InvokeMethod(messageCollectorType, null, "GetAllPendingScreenshotFiles",
                BindingFlags.Static | BindingFlags.Public) as IEnumerable<string>).FirstOrDefault();
        }

        private void InvokeScreenshotCapture() {
            var gaugeScreenshotsType = _assemblyLoader.GetLibType(LibType.GaugeScreenshots);
            _reflectionWrapper.InvokeMethod(gaugeScreenshotsType, null, "Capture",
                BindingFlags.Static | BindingFlags.Public);
        }
    }
}