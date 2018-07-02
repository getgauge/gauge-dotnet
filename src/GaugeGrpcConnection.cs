using System;
using System.Threading.Tasks;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet
{
    public class GaugeGrpcConnection : lspService.lspServiceBase
    {
        private readonly Server _server;
        private readonly StaticLoader _loader;
        private readonly MessageProcessorFactory _factory;
        private IStepRegistry _registry;

        public GaugeGrpcConnection(Server server, StaticLoader loader, MessageProcessorFactory factory)
        {
            _server = server;
            _loader = loader;
            _factory = factory;
            _registry = loader.GetStepRegistry();
        }

        public override Task<Empty> CacheFile(CacheFileRequest request, ServerCallContext context)
        {
            _loader.ReloadSteps(request.Content, request.FilePath);
            return Task.FromResult(new Empty());
        }

        public override Task<ImplementationFileGlobPatternResponse> GetGlobPatterns(Empty request,
            ServerCallContext context)
        {
            var response = new ImplementationFileGlobPatternResponse()
            {
                GlobPatterns =
                {
                    $"{Utils.GaugeProjectRoot}/**/*.cs"
                }
            };
            return Task.FromResult(response);
        }

        public override Task<ImplementationFileListResponse> GetImplementationFiles(Empty request,
            ServerCallContext context)
        {
            var response = new ImplementationFileListResponse()
            {
                ImplementationFilePaths =
                {
                    "StepImplementation.cs"
                }
            };
            return Task.FromResult(response);
        }

        public override Task<StepNameResponse> GetStepName(StepNameRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepNameRequest)
                .Process(new Message() {StepNameRequest = request});
            return Task.FromResult(response.StepNameResponse);
        }

        public override Task<StepNamesResponse> GetStepNames(StepNamesRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepNamesRequest)
                .Process(new Message() {StepNamesRequest = request});
            return Task.FromResult(response.StepNamesResponse);
        }

        public override Task<StepPositionsResponse> GetStepPositions(StepPositionsRequest request,
            ServerCallContext context)
        {

            return base.GetStepPositions(request, context);
        }

        public override Task<FileDiff> ImplementStub(StubImplementationCodeRequest request, ServerCallContext context)
        {
            return Task.FromResult(new FileDiff()
            {
                FilePath = "StepImplementation.cs",
                TextDiffs =
                {
                    new TextDiff()
                    {
                        Span = new Span()
                        {
                            Start = 0,
                            StartChar = 0,
                            End = 10,
                            EndChar = 10
                        },
                        Content = "Hello Word"
                    }
                }
            });
        }

        public override Task<Empty> KillProcess(KillProcessRequest request, ServerCallContext context)
        {
            _server.ShutdownAsync().Wait();
            return Task.FromResult(new Empty());
        }

        public override Task<RefactorResponse> Refactor(RefactorRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.RefactorRequest).Process(new Message({RefactorRequest = request});;
            return Task.FromResult(response.RefactorResponse);
        }

        public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepValidateRequest)
                .Process(new Message() {StepValidateRequest = request});    
            return Task.FromResult(response.StepValidateResponse);
        }
    }
}