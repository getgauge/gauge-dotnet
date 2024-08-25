/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Executor;
using Gauge.Dotnet.Models;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet;

internal class AuthoringRunnerServiceHandler : Runner.RunnerBase
{
    private readonly IExecutor _executor;
    private readonly IHostApplicationLifetime _lifetime;
    protected IStepRegistry _stepRegistry;

    public AuthoringRunnerServiceHandler(IExecutor executor, IHostApplicationLifetime lifetime, IStepRegistry stepRegistry)
    {
        _executor = executor;
        _lifetime = lifetime;
        _stepRegistry = stepRegistry;
    }

    public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
    {
        return _executor.Execute<StepValidateRequest, StepValidateResponse>(request);
    }

    public override Task<Empty> CacheFile(CacheFileRequest request, ServerCallContext context)
    {
        return _executor.Execute<CacheFileRequest, Empty>(request);
    }

    public override Task<ImplementationFileGlobPatternResponse> GetGlobPatterns(Empty request, ServerCallContext context)
    {
        return _executor.Execute<Empty, ImplementationFileGlobPatternResponse>(request);
    }

    public override Task<ImplementationFileListResponse> GetImplementationFiles(Empty request, ServerCallContext context)
    {
        return _executor.Execute<Empty, ImplementationFileListResponse>(request);
    }

    public override Task<StepNameResponse> GetStepName(StepNameRequest request, ServerCallContext context)
    {
        return _executor.Execute<StepNameRequest, StepNameResponse>(request);
    }

    public override Task<StepNamesResponse> GetStepNames(StepNamesRequest request, ServerCallContext context)
    {
        return _executor.Execute<StepNamesRequest, StepNamesResponse>(request);
    }

    public override Task<StepPositionsResponse> GetStepPositions(StepPositionsRequest request, ServerCallContext context)
    {
        return _executor.Execute<StepPositionsRequest, StepPositionsResponse>(request);
    }

    public override Task<FileDiff> ImplementStub(StubImplementationCodeRequest request, ServerCallContext context)
    {
        return _executor.Execute<StubImplementationCodeRequest, FileDiff>(request);
    }

    public override Task<RefactorResponse> Refactor(RefactorRequest request, ServerCallContext context)
    {
        return _executor.Execute<RefactorRequest, RefactorResponse>(request);
    }

    public override Task<Empty> Kill(KillProcessRequest request, ServerCallContext context)
    {
        Logger.Debug("KillProcessrequest received");
        _lifetime.StopApplication();
        return Task.FromResult(new Empty());
    }
}