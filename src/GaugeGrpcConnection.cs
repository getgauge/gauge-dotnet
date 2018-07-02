using System;
using System.Threading.Tasks;
using Gauge.CSharp.Core;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet
{
    public class GaugeGrpcConnection : lspService.lspServiceBase
    {
        private readonly Server _server;

        public GaugeGrpcConnection(Server server)
        {
            _server = server;
        }

        public override Task<Empty> CacheFile(CacheFileRequest request, ServerCallContext context)
        {
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
            return Task.FromResult(new StepNameResponse()
            {
                HasAlias = false,
                IsStepPresent = true,
                FileName = "StepImplementation.cs",
                Span = new Span()
                {
                    Start = 0,
                    StartChar = 0,
                    End = 10,
                    EndChar = 10
                }
            });
        }

        public override Task<StepNamesResponse> GetStepNames(StepNamesRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StepNamesResponse()
            {
                Steps = {"hello", "foo", "bar"}
            });
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
            return base.Refactor(request, context);
        }

        public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StepValidateResponse()
            {
                IsValid = true,
            });
        }
    }
}