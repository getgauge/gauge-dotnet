/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Diagnostics;
using System.Reflection;
using Gauge.Dotnet.DataStore;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet.Executors;

public class ExecutionOrchestrator : IExecutionOrchestrator
{
    private readonly IAssemblyLoader _assemblyLoader;
    private readonly object _classInstanceManager;
    private readonly IHookExecutor _hookExecutor;
    private readonly IReflectionWrapper _reflectionWrapper;
    private readonly IStepExecutor _stepExecutor;
    private readonly IConfiguration _config;
    private readonly IDataStoreFactory _dataStoreFactory;
    private readonly ILogger<ExecutionOrchestrator> _logger;

    public ExecutionOrchestrator(IReflectionWrapper reflectionWrapper, IAssemblyLoader assemblyLoader, IHookExecutor hookExecutor, IStepExecutor stepExecutor,
        IConfiguration config, ILogger<ExecutionOrchestrator> logger, IDataStoreFactory dataStoreFactory)
    {
        _reflectionWrapper = reflectionWrapper;
        _assemblyLoader = assemblyLoader;
        _classInstanceManager = assemblyLoader.GetClassInstanceManager();
        _hookExecutor = hookExecutor;
        _stepExecutor = stepExecutor;
        _config = config;
        _dataStoreFactory = dataStoreFactory;
        _logger = logger;
    }


    [DebuggerHidden]
    public async Task<ProtoExecutionResult> ExecuteStep(GaugeMethod method, int streamId, params string[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var executionResult = await _stepExecutor.Execute(method, streamId, args);
        return BuildResult(stopwatch, executionResult, streamId);
    }

    public void ClearCache()
    {
        _reflectionWrapper.InvokeMethod(_assemblyLoader.ClassInstanceManagerType, _classInstanceManager, "ClearCache");
    }

    public void StartExecutionScope(string tag)
    {
        _reflectionWrapper.InvokeMethod(_assemblyLoader.ClassInstanceManagerType, _classInstanceManager, "StartScope", tag);
    }

    public void CloseExecutionScope()
    {
        _reflectionWrapper.InvokeMethod(_assemblyLoader.ClassInstanceManagerType, _classInstanceManager, "CloseScope");
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
    public async Task<ProtoExecutionResult> ExecuteHooks(string hookType, HooksStrategy strategy, IList<string> applicableTags, int streamId, ExecutionInfo info)
    {
        var stopwatch = Stopwatch.StartNew();
        var executionResult = await _hookExecutor.Execute(hookType, strategy, applicableTags, streamId, info);
        return BuildResult(stopwatch, executionResult, streamId);
    }

    private ProtoExecutionResult BuildResult(Stopwatch stopwatch, ExecutionResult executionResult, int streamId)
    {
        var result = new ProtoExecutionResult
        {
            Failed = false,
            ExecutionTime = stopwatch.ElapsedMilliseconds,
            SkipScenario = executionResult.SkipScenario
        };
        var allPendingMessages = GetAllPendingMessages().Where(m => m != null);
        result.Message.AddRange(allPendingMessages);
        var allPendingScreenShotFiles = GetAllPendingScreenshotFiles().Where(s => s != null);
        result.ScreenshotFiles.AddRange(allPendingScreenShotFiles);

        // If runtime skipped scenario return Error message and stack info
        if (!string.IsNullOrEmpty(executionResult.ExceptionMessage))
        {
            result.ErrorMessage = executionResult.ExceptionMessage;
        }
        if (!string.IsNullOrEmpty(executionResult.StackTrace))
        {
            result.StackTrace = executionResult.StackTrace;
        }
        if (executionResult.Success) return result;

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        result.Failed = true;
        if (_config.ScreenshotOnFailure())
        {
            var screenshotFile = TryScreenCapture(streamId);
            if (!string.IsNullOrEmpty(screenshotFile))
            {
                result.FailureScreenshotFile = screenshotFile;
            }
        }

        result.RecoverableError = executionResult.Recoverable;
        result.ExecutionTime = elapsedMilliseconds;
        return result;
    }

    private string TryScreenCapture(int streamId)
    {
        try
        {
            InvokeScreenshotCapture(streamId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unable to capture screenshot, CustomScreenshotWriter is probably not set.({Message})\n{StackTrace}", ex.Message, ex.StackTrace);
            return null;
        }
        var messageCollectorType = _assemblyLoader.GetLibType(LibType.ScreenshotFilesCollector);
        return (_reflectionWrapper.InvokeMethod(messageCollectorType, null, "GetAllPendingScreenshotFiles",
            BindingFlags.Static | BindingFlags.Public) as IEnumerable<string>).FirstOrDefault();
    }

    private void InvokeScreenshotCapture(int streamId)
    {
        var gaugeScreenshotsType = _assemblyLoader.GetLibType(LibType.GaugeScreenshots);
        var dataStores = _dataStoreFactory.GetDataStoresByStream(streamId);
        var methodInfo = _reflectionWrapper.GetMethod(gaugeScreenshotsType, "CaptureWithDataStores", BindingFlags.Static | BindingFlags.Public);
        _reflectionWrapper.Invoke(methodInfo, null, _dataStoreFactory.SuiteDataStore, dataStores.GetValueOrDefault(DataStoreType.Spec),
            dataStores.GetValueOrDefault(DataStoreType.Scenario));
        return;
    }
}