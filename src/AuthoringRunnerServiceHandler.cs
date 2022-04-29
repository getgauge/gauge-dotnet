/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Threading.Tasks;
using Gauge.Dotnet.Executor;
using Gauge.Dotnet.Helpers;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using Grpc.Core;
using Microsoft.Extensions.Hosting;

namespace Gauge.Dotnet
{
    internal class AuthoringRunnerServiceHandler : Runner.RunnerBase
    {
        private readonly int DefaultExecutionStream = 1;

        private readonly IStaticLoader _loader;
        protected readonly ExecutorPool _pool;
        private readonly IHostApplicationLifetime lifetime;
        protected IStepRegistry _stepRegistry;

        private StepValidationProcessor stepValidateRequestProcessor;
        private StepNameProcessor stepNameRequestProcessor;
        private RefactorProcessor refactorRequestProcessor;
        private CacheFileProcessor cacheFileRequestProcessor;
        private StubImplementationCodeProcessor stubImplementationCodeRequestProcessor;
        private StepPositionsProcessor stepPositionsRequestProcessor;
        private StepNamesProcessor stepNamesRequestProcessor;

        public AuthoringRunnerServiceHandler(IStaticLoader loader, ExecutorPool pool, IHostApplicationLifetime lifetime)
        {
            this._pool = pool;
            this.lifetime = lifetime;
            this._loader = loader;
            _stepRegistry = loader.GetStepRegistry();
            this.InitializeMessageProcessors();
        }

        public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
        {
            return _pool.Execute(DefaultExecutionStream, () => this.stepValidateRequestProcessor.Process(request));
        }


        public override Task<Empty> CacheFile(CacheFileRequest request, ServerCallContext context)
        {
            return _pool.Execute(1, () => this.cacheFileRequestProcessor.Process(request));
        }

        public override Task<ImplementationFileGlobPatternResponse> GetGlobPatterns(Empty request, ServerCallContext context)
        {
            var response = new ImplementationFileGlobPatternResponse();
            response.GlobPatterns.Add(FileHelper.GetImplementationGlobPatterns());
            return _pool.Execute(1, () => response);
        }


        public override Task<ImplementationFileListResponse> GetImplementationFiles(Empty request, ServerCallContext context)
        {
            return _pool.Execute(DefaultExecutionStream,() => {
                var response = new ImplementationFileListResponse();
                response.ImplementationFilePaths.AddRange(FileHelper.GetImplementationFiles());
                return response;
            });
        }

        public override Task<StepNameResponse> GetStepName(StepNameRequest request, ServerCallContext context)
        {
            return _pool.Execute(DefaultExecutionStream, () => this.stepNameRequestProcessor.Process(request));
        }

        public override Task<StepNamesResponse> GetStepNames(StepNamesRequest request, ServerCallContext context)
        {
            return _pool.Execute(DefaultExecutionStream, () => this.stepNamesRequestProcessor.Process(request));
        }

        public override Task<StepPositionsResponse> GetStepPositions(StepPositionsRequest request, ServerCallContext context)
        {
            return _pool.Execute(DefaultExecutionStream, () => this.stepPositionsRequestProcessor.Process(request));
        }

        public override Task<FileDiff> ImplementStub(StubImplementationCodeRequest request, ServerCallContext context)
        {
            return _pool.Execute(DefaultExecutionStream, () => this.stubImplementationCodeRequestProcessor.Process(request));
        }

        public override Task<RefactorResponse> Refactor(RefactorRequest request, ServerCallContext context)
        {
            return _pool.Execute(DefaultExecutionStream, () => this.refactorRequestProcessor.Process(request));
        }

        protected void InitializeMessageProcessors()
        {
            this.stepValidateRequestProcessor = new StepValidationProcessor(_stepRegistry);
            this.stepNameRequestProcessor = new StepNameProcessor(_stepRegistry);
            this.refactorRequestProcessor = new RefactorProcessor(_stepRegistry);
            this.cacheFileRequestProcessor = new CacheFileProcessor(_loader);
            this.stubImplementationCodeRequestProcessor = new StubImplementationCodeProcessor();
            this.stepPositionsRequestProcessor = new StepPositionsProcessor(_stepRegistry);
            this.stepNamesRequestProcessor = new StepNamesProcessor(_stepRegistry);
        }

        public override Task<Empty> Kill(KillProcessRequest request, ServerCallContext context)
        {
            try
            {
                Logger.Debug("KillProcessrequest received");
                return Task.FromResult(new Empty());
            }
            finally
            {
                _pool.Dispose();
                Task.Delay(500).ContinueWith((_) => lifetime.StopApplication());
            }
        }
    }
}