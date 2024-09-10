/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Models;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet;

internal class AuthoringRunnerServiceHandler : Runner.RunnerBase
{
    private const int DefaultStream = 1;
    protected IExecutor Executor { get; private init; }
    private readonly IHostApplicationLifetime _lifetime;
    protected IStepRegistry _stepRegistry;
    protected ILogger<AuthoringRunnerServiceHandler> _logger;

    public AuthoringRunnerServiceHandler(IExecutor executor, IHostApplicationLifetime lifetime, IStepRegistry stepRegistry, ILogger<AuthoringRunnerServiceHandler> logger)
    {
        Executor = executor;
        _lifetime = lifetime;
        _stepRegistry = stepRegistry;
        _logger = logger;
    }

    public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
    {
        return Executor.Execute<StepValidateRequest, StepValidateResponse>(DefaultStream, request);
    }

    public override Task<Empty> CacheFile(CacheFileRequest request, ServerCallContext context)
    {
        return Executor.Execute<CacheFileRequest, Empty>(DefaultStream, request);
    }

    public override Task<ImplementationFileGlobPatternResponse> GetGlobPatterns(Empty request, ServerCallContext context)
    {
        return Executor.Execute<Empty, ImplementationFileGlobPatternResponse>(DefaultStream, request);
    }

    public override Task<ImplementationFileListResponse> GetImplementationFiles(Empty request, ServerCallContext context)
    {
        return Executor.Execute<Empty, ImplementationFileListResponse>(DefaultStream, request);
    }

    public override Task<StepNameResponse> GetStepName(StepNameRequest request, ServerCallContext context)
    {
        return Executor.Execute<StepNameRequest, StepNameResponse>(DefaultStream, request);
    }

    public override Task<StepNamesResponse> GetStepNames(StepNamesRequest request, ServerCallContext context)
    {
        return Executor.Execute<StepNamesRequest, StepNamesResponse>(DefaultStream, request);
    }

    public override Task<StepPositionsResponse> GetStepPositions(StepPositionsRequest request, ServerCallContext context)
    {
        return Executor.Execute<StepPositionsRequest, StepPositionsResponse>(DefaultStream, request);
    }

    public override Task<FileDiff> ImplementStub(StubImplementationCodeRequest request, ServerCallContext context)
    {
        return Executor.Execute<StubImplementationCodeRequest, FileDiff>(DefaultStream, request);
    }

    public override Task<RefactorResponse> Refactor(RefactorRequest request, ServerCallContext context)
    {
        return Executor.Execute<RefactorRequest, RefactorResponse>(DefaultStream, request);
    }

    public override Task<Empty> Kill(KillProcessRequest request, ServerCallContext context)
    {
        _logger.LogDebug("KillProcessrequest received");
        _lifetime.StopApplication();
        return Task.FromResult(new Empty());
    }
}