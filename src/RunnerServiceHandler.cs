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
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet
{
    internal class RunnerServiceHandler: Runner.RunnerBase
    {
        private IStaticLoader _loader;
        private Server _server;
        private readonly IActivatorWrapper _activatorWrapper;
        private readonly IReflectionWrapper _reflectionWrapper;
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IStepRegistry _stepRegistry;
        private StepValidationProcessor stepValidateRequestProcessor;
        private StepNameProcessor stepNameRequestProcessor;
        private RefactorProcessor refactorRequestProcessor;
        private CacheFileProcessor cacheFileRequestProcessor;
        private StubImplementationCodeProcessor stubImplementationCodeRequestProcessor;
        private StepPositionsProcessor stepPositionsRequestProcessor;
        private StepNamesProcessor stepNamesRequestProcessor;
        private ExecutionStartingProcessor executionStartingProcessor;
        private ExecutionEndingProcessor executionEndingProcessor;
        private SpecExecutionStartingProcessor specExecutionStartingProcessor;
        private SpecExecutionEndingProcessor specExecutionEndingProcessor;
        private ScenarioExecutionStartingProcessor scenarioExecutionStartingProcessor;
        private ScenarioExecutionEndingProcessor scenarioExecutionEndingProcessor;
        private StepExecutionStartingProcessor stepExecutionStartingProcessor;
        private StepExecutionEndingProcessor stepExecutionEndingProcessor;
        private ExecuteStepProcessor executeStepProcessor;
        private ScenarioDataStoreInitProcessor scenarioDataStoreInitProcessor;
        private SpecDataStoreInitProcessor specDataStoreInitProcessor;
        private SuiteDataStoreInitProcessor suiteDataStoreInitProcessor;


        public RunnerServiceHandler(IActivatorWrapper activationWrapper, IReflectionWrapper reflectionWrapper, IAssemblyLoader assemblyLoader, IStaticLoader loader, Server server)
        {
            this._loader = loader;
            this._server = server;
            this._activatorWrapper = activationWrapper;
            this._reflectionWrapper = reflectionWrapper;
            this._assemblyLoader = assemblyLoader;
            _stepRegistry = assemblyLoader.GetStepRegistry();
            this.InitializeMessageProcessors();
        }

        public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepValidateRequestProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> ExecuteStep(ExecuteStepRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.executeStepProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishExecution(ExecutionEndingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.executionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishScenarioExecution(ScenarioExecutionEndingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.scenarioExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishSpecExecution(SpecExecutionEndingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.specExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishStepExecution(StepExecutionEndingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepExecutionEndingProcessor.Process(request));
        }


        public override Task<ExecutionStatusResponse> InitializeScenarioDataStore(Empty request, ServerCallContext context)
        {
            return Task.FromResult(this.scenarioDataStoreInitProcessor.Process());
        }

        public override Task<ExecutionStatusResponse> InitializeSpecDataStore(Empty request, ServerCallContext context)
        {
            return Task.FromResult(this.specDataStoreInitProcessor.Process());
        }

        public override Task<ExecutionStatusResponse> InitializeSuiteDataStore(Empty request, ServerCallContext context)
        {
            InitializeExecutionMessageHandlers();
            return Task.FromResult(this.suiteDataStoreInitProcessor.Process());
        }

        public override Task<ExecutionStatusResponse> StartExecution(ExecutionStartingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.executionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StartScenarioExecution(ScenarioExecutionStartingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.scenarioExecutionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StartSpecExecution(SpecExecutionStartingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.specExecutionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StartStepExecution(StepExecutionStartingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepExecutionStartingProcessor.Process(request));
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

        public override Task<Empty> Kill(KillProcessRequest request, ServerCallContext context)
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

        private void InitializeExecutionMessageHandlers()
        {
            var tableFormatter = new TableFormatter(this._assemblyLoader, this._activatorWrapper);
            var classInstanceManager = this._assemblyLoader.GetClassInstanceManager();
            var executionOrchestrator = new ExecutionOrchestrator(this._reflectionWrapper, this._assemblyLoader, this._activatorWrapper,
                classInstanceManager,
                new HookExecutor(this._assemblyLoader, this._reflectionWrapper, classInstanceManager),
                new StepExecutor(this._assemblyLoader, this._reflectionWrapper, classInstanceManager));

            this.executionStartingProcessor = new ExecutionStartingProcessor(executionOrchestrator);
            this.executionEndingProcessor = new ExecutionEndingProcessor(executionOrchestrator);
            this.specExecutionStartingProcessor = new SpecExecutionStartingProcessor(executionOrchestrator);
            this.specExecutionEndingProcessor = new SpecExecutionEndingProcessor(executionOrchestrator);
            this.scenarioExecutionStartingProcessor = new ScenarioExecutionStartingProcessor(executionOrchestrator);
            this.scenarioExecutionEndingProcessor = new ScenarioExecutionEndingProcessor(executionOrchestrator);
            this.stepExecutionStartingProcessor = new StepExecutionStartingProcessor(executionOrchestrator);
            this.stepExecutionEndingProcessor = new StepExecutionEndingProcessor(executionOrchestrator);
            this.executeStepProcessor = new ExecuteStepProcessor(_stepRegistry, executionOrchestrator, tableFormatter);
            this.scenarioDataStoreInitProcessor = new ScenarioDataStoreInitProcessor(this._assemblyLoader);
            this.specDataStoreInitProcessor = new SpecDataStoreInitProcessor(this._assemblyLoader);
            this.suiteDataStoreInitProcessor = new SuiteDataStoreInitProcessor(this._assemblyLoader);
        }

    }
}