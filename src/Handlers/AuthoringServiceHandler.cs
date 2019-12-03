
using System.Threading.Tasks;
using Gauge.Dotnet.Helpers;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet.Handlers
{
    internal class AuthoringServiceHandler: Authoring.AuthoringBase
    {

        private IStaticLoader _loader;
        private IStepRegistry _stepRegistry;
        private StepValidationProcessor stepValidateRequestProcessor;
        private StepNameProcessor stepNameRequestProcessor;
        private RefactorProcessor refactorRequestProcessor;
        private CacheFileProcessor cacheFileRequestProcessor;
        private StubImplementationCodeProcessor stubImplementationCodeRequestProcessor;
        private StepPositionsProcessor stepPositionsRequestProcessor;
        private StepNamesProcessor stepNamesRequestProcessor;



        public AuthoringServiceHandler(IStaticLoader loader)
        {
            _loader = loader;
            _stepRegistry = loader.GetStepRegistry();
            this.InitializeMessageProcessors();
        }

        public override Task<Empty> CacheFile(CacheFileRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.cacheFileRequestProcessor.Process(request));
        }


        public override Task<ImplementationFileGlobPatternResponse> GetGlobPatterns(Empty request, ServerCallContext context)
        {
            var response = new ImplementationFileGlobPatternResponse();
            response.GlobPatterns.Add(FileHelper.GetImplementationGlobPatterns());
            return Task.FromResult(response);
        }


        public override Task<ImplementationFileListResponse> GetImplementationFiles(Empty request, ServerCallContext context)
        {
            var response = new ImplementationFileListResponse();
            response.ImplementationFilePaths.AddRange(FileHelper.GetImplementationFiles());
            return Task.FromResult(response);
        }

        public override Task<StepNameResponse> GetStepName(StepNameRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepNameRequestProcessor.Process(request));
        }

        public override Task<StepNamesResponse> GetStepNames(StepNamesRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepNamesRequestProcessor.Process(request));
        }

        public override Task<StepPositionsResponse> GetStepPositions(StepPositionsRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepPositionsRequestProcessor.Process(request));
        }

        public override Task<FileDiff> ImplementStub(StubImplementationCodeRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stubImplementationCodeRequestProcessor.Process(request));
        }

        public override Task<RefactorResponse> Refactor(RefactorRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.refactorRequestProcessor.Process(request));
        }

        private void InitializeMessageProcessors()
        {
            this.stepValidateRequestProcessor = new StepValidationProcessor(_stepRegistry);
            this.stepNameRequestProcessor = new StepNameProcessor(_stepRegistry);
            this.refactorRequestProcessor = new RefactorProcessor(_stepRegistry);
            this.cacheFileRequestProcessor = new CacheFileProcessor(_loader);
            this.stubImplementationCodeRequestProcessor = new StubImplementationCodeProcessor();
            this.stepPositionsRequestProcessor = new StepPositionsProcessor(_stepRegistry);
            this.stepNamesRequestProcessor = new StepNamesProcessor(_stepRegistry);
        }
    }
}