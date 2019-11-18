using System.Threading.Tasks;
using Gauge.Messages;
using Grpc.Core;
using Gauge.Dotnet.Helpers;
using System.Threading;
using System;

namespace Gauge.Dotnet
{
    class RunnerServiceHandler : Runner.RunnerBase
    {

        private readonly MessageProcessorFactory _factory;
        private readonly Server _server;

        public RunnerServiceHandler(Server server, MessageProcessorFactory factory)
        {
            _server = server;
            _factory = factory;
        }

        public override Task<Empty> CacheFile(CacheFileRequest request, ServerCallContext context)
        {
            _factory.GetProcessor(Message.Types.MessageType.CacheFileRequest)
                .Process(new Message { CacheFileRequest = request });
            return Task.FromResult(new Empty());
        }

        public override Task<ExecutionStatusResponse> ExecuteStep(ExecuteStepRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.ExecuteStep)
                .Process(new Message { ExecuteStepRequest = request });
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> ExecutionEnding(ExecutionEndingRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.ExecutionEnding)
                .Process(new Message { ExecutionEndingRequest = request });
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> ExecutionStarting(ExecutionStartingRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.ExecutionStarting)
                .Process(new Message { ExecutionStartingRequest = request });
            return Task.FromResult(response.ExecutionStatusResponse);
        }


        public override Task<ImplementationFileGlobPatternResponse> GetGlobPatterns(Empty request, ServerCallContext context)
        {
            var response = new ImplementationFileGlobPatternResponse();
            response.GlobPatterns.Add(FileHelper.GetImplementationGlobPatterns());
            return Task.FromResult(response);
        }

        public override Task<ImplementationFileListResponse> GetImplementationFiles(Empty request,
            ServerCallContext context)
        {
            var response = new ImplementationFileListResponse();
            response.ImplementationFilePaths.AddRange(FileHelper.GetImplementationFiles());
            return Task.FromResult(response);
        }

        public override Task<StepNameResponse> GetStepName(StepNameRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepNameRequest)
                .Process(new Message { StepNameRequest = request });
            return Task.FromResult(response.StepNameResponse);
        }

        public override Task<StepNamesResponse> GetStepNames(StepNamesRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepNamesRequest)
                .Process(new Message { StepNamesRequest = request });
            return Task.FromResult(response.StepNamesResponse);
        }

        public override Task<StepPositionsResponse> GetStepPositions(StepPositionsRequest request,
            ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepPositionsRequest)
                .Process(new Message { StepPositionsRequest = request });
            return Task.FromResult(response.StepPositionsResponse);
        }

        public override Task<FileDiff> ImplementStub(StubImplementationCodeRequest request, ServerCallContext context)
        {
            var respone = _factory.GetProcessor(Message.Types.MessageType.StubImplementationCodeRequest)
                .Process(new Message { StubImplementationCodeRequest = request });
            return Task.FromResult(respone.FileDiff);
        }

        public override Task<Empty> KillProcess(KillProcessRequest request, ServerCallContext context)
        {
            try
            {
                Logger.Debug("KillProcessrequest received");
                return Task.FromResult(new Empty());
            }
            finally
            {
                _server.ShutdownAsync();
            }
        }


        public override Task<RefactorResponse> Refactor(RefactorRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.RefactorRequest)
                .Process(new Message { RefactorRequest = request });
            return Task.FromResult(response.RefactorResponse);
        }

        public override Task<ExecutionStatusResponse> ScenarioDataStoreInit(Empty request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.ScenarioDataStoreInit)
            .Process(new Message());
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> ScenarioExecutionEnding(ScenarioExecutionEndingRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.ScenarioExecutionEnding)
                .Process(new Message { ScenarioExecutionEndingRequest = request });
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> ScenarioExecutionStarting(ScenarioExecutionStartingRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.ScenarioExecutionStarting)
                .Process(new Message { ScenarioExecutionStartingRequest = request });
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> SpecDataStoreInit(Empty request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.SpecDataStoreInit)
            .Process(new Message());
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> SpecExecutionEnding(SpecExecutionEndingRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.SpecExecutionEnding)
                .Process(new Message { SpecExecutionEndingRequest = request });
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> SpecExecutionStarting(SpecExecutionStartingRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.SpecExecutionStarting)
                .Process(new Message { SpecExecutionStartingRequest = request });
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> StepExecutionEnding(StepExecutionEndingRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepExecutionEnding)
                .Process(new Message { StepExecutionEndingRequest = request });
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> StepExecutionStarting(StepExecutionStartingRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepExecutionStarting)
                .Process(new Message { StepExecutionStartingRequest = request });
            ;
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<ExecutionStatusResponse> SuiteDataStoreInit(Empty request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.SuiteDataStoreInit, true)
            .Process(new Message());
            return Task.FromResult(response.ExecutionStatusResponse);
        }

        public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepValidateRequest)
                .Process(new Message { StepValidateRequest = request });
            return Task.FromResult(response.StepValidateResponse);
        }
    }
}
