/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet;

internal class ExecutableRunnerServiceHandler : AuthoringRunnerServiceHandler
{
    private readonly IConfiguration _config;

    public IAssemblyLoader AssemblyLoader { get; private set; }

    public ExecutableRunnerServiceHandler(IExecutor executor, IAssemblyLoader assemblyLoader, IHostApplicationLifetime lifetime, IStepRegistry stepRegistry, IConfiguration config)
        : base(executor, lifetime, stepRegistry)
    {
        _config = config;
        AssemblyLoader = assemblyLoader;
    }
    public override Task<ExecutionStatusResponse> InitializeSuiteDataStore(SuiteDataStoreInitRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        Logger.Info($"Processing init suite data store processor. Stream {request.Stream}");
        return Executor.Execute<SuiteDataStoreInitRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> ExecuteStep(ExecuteStepRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<ExecuteStepRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> FinishExecution(ExecutionEndingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<ExecutionEndingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> FinishScenarioExecution(ScenarioExecutionEndingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<ScenarioExecutionEndingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> FinishSpecExecution(SpecExecutionEndingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<SpecExecutionEndingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> FinishStepExecution(StepExecutionEndingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<StepExecutionEndingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> InitializeScenarioDataStore(ScenarioDataStoreInitRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<ScenarioDataStoreInitRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> InitializeSpecDataStore(SpecDataStoreInitRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<SpecDataStoreInitRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> StartExecution(ExecutionStartingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<ExecutionStartingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> StartScenarioExecution(ScenarioExecutionStartingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<ScenarioExecutionStartingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> StartSpecExecution(SpecExecutionStartingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<SpecExecutionStartingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> StartStepExecution(StepExecutionStartingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<StepExecutionStartingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> NotifyConceptExecutionStarting(ConceptExecutionStartingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<ConceptExecutionStartingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    public override Task<ExecutionStatusResponse> NotifyConceptExecutionEnding(ConceptExecutionEndingRequest request, ServerCallContext context)
    {
        request.Stream = GetStream(request.Stream);
        return Executor.Execute<ConceptExecutionEndingRequest, ExecutionStatusResponse>(request.Stream, request);
    }

    private int GetStream(int stream)
    {
        if (!_config.IsMultithreading())
        {
            return 1;
        }
        return Math.Max(stream, 1);
    }
}