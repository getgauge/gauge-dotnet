// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-Dotnet.
//
// Gauge-Dotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-Dotnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-Dotnet.  If not, see <http://www.gnu.org/licenses/>.

using System.Threading.Tasks;
using Gauge.Dotnet.Helpers;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet
{
    public class GaugeGrpcConnection : lspService.lspServiceBase
    {
        private readonly MessageProcessorFactory _factory;
        private readonly Server _server;

        public GaugeGrpcConnection(Server server, MessageProcessorFactory factory)
        {
            _server = server;
            _factory = factory;
        }

        public override Task<Empty> CacheFile(CacheFileRequest request, ServerCallContext context)
        {
            _factory.GetProcessor(Message.Types.MessageType.CacheFileRequest)
                .Process(new Message {CacheFileRequest = request});
            return Task.FromResult(new Empty());
        }

        public override Task<ImplementationFileGlobPatternResponse> GetGlobPatterns(Empty request,
            ServerCallContext context)
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
                .Process(new Message {StepNameRequest = request});
            return Task.FromResult(response.StepNameResponse);
        }

        public override Task<StepNamesResponse> GetStepNames(StepNamesRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepNamesRequest)
                .Process(new Message {StepNamesRequest = request});
            return Task.FromResult(response.StepNamesResponse);
        }

        public override Task<StepPositionsResponse> GetStepPositions(StepPositionsRequest request,
            ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepPositionsRequest)
                .Process(new Message {StepPositionsRequest = request});
            return Task.FromResult(response.StepPositionsResponse);
        }

        public override Task<FileDiff> ImplementStub(StubImplementationCodeRequest request, ServerCallContext context)
        {
            var respone = _factory.GetProcessor(Message.Types.MessageType.StubImplementationCodeRequest)
                .Process(new Message {StubImplementationCodeRequest = request});
            return Task.FromResult(respone.FileDiff);
        }

        public override Task<Empty> KillProcess(KillProcessRequest request, ServerCallContext context)
        {
            _server.KillAsync().Wait();
            return Task.FromResult(new Empty());
        }

        public override Task<RefactorResponse> Refactor(RefactorRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.RefactorRequest)
                .Process(new Message {RefactorRequest = request});
            ;
            return Task.FromResult(response.RefactorResponse);
        }

        public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
        {
            var response = _factory.GetProcessor(Message.Types.MessageType.StepValidateRequest)
                .Process(new Message {StepValidateRequest = request});
            return Task.FromResult(response.StepValidateResponse);
        }
    }
}