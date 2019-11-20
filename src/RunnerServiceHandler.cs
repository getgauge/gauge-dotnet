using System.Threading.Tasks;
using Gauge.Messages;
using Grpc.Core;
using Gauge.Dotnet.Helpers;
using Gauge.Dotnet.Wrappers;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;

namespace Gauge.Dotnet
{
    class RunnerServiceHandler : Runner.RunnerBase
    {

        private readonly Server _server;
        private readonly IStaticLoader _loader;
        private IStepRegistry _stepRegistry;
        private StepValidationProcessor stepValidateRequestProcessor;
        private StepNameProcessor stepNameRequestProcessor;
        private RefactorProcessor refactorRequestProcessor;
        private CacheFileProcessor cacheFileRequestProcessor;
        private StubImplementationCodeProcessor stubImplementationCodeRequestProcessor;
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
        private StepPositionsProcessor stepPositionsRequestProcessor;

        private StepNamesProcessor stepNamesRequestProcessor;

        public RunnerServiceHandler(Server server, IStaticLoader loader)
        {
            this._server = server;
            this._loader = loader;
            this._stepRegistry = _loader.GetStepRegistry();
            this.InitializeMessageProcessors();
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

        private void InitializeExecutionMessageHandlers(IReflectionWrapper reflectionWrapper,
            IAssemblyLoader assemblyLoader, IActivatorWrapper activatorWrapper, ITableFormatter tableFormatter,
            object classInstanceManager)
        {
            var executionOrchestrator = new ExecutionOrchestrator(reflectionWrapper, assemblyLoader, activatorWrapper,
                classInstanceManager,
                new HookExecutor(assemblyLoader, reflectionWrapper, classInstanceManager),
                new StepExecutor(assemblyLoader, reflectionWrapper, classInstanceManager));

            this.executionStartingProcessor = new ExecutionStartingProcessor(executionOrchestrator);
            this.executionEndingProcessor = new ExecutionEndingProcessor(executionOrchestrator);
            this.specExecutionStartingProcessor = new SpecExecutionStartingProcessor(executionOrchestrator);
            this.specExecutionEndingProcessor = new SpecExecutionEndingProcessor(executionOrchestrator);
            this.scenarioExecutionStartingProcessor = new ScenarioExecutionStartingProcessor(executionOrchestrator);
            this.scenarioExecutionEndingProcessor = new ScenarioExecutionEndingProcessor(executionOrchestrator);
            this.stepExecutionStartingProcessor = new StepExecutionStartingProcessor(executionOrchestrator);
            this.stepExecutionEndingProcessor = new StepExecutionEndingProcessor(executionOrchestrator);
            this.executeStepProcessor = new ExecuteStepProcessor(_stepRegistry, executionOrchestrator, tableFormatter);
            this.scenarioDataStoreInitProcessor = new ScenarioDataStoreInitProcessor(assemblyLoader);
            this.specDataStoreInitProcessor = new SpecDataStoreInitProcessor(assemblyLoader);
            this.suiteDataStoreInitProcessor = new SuiteDataStoreInitProcessor(assemblyLoader);
        }

        public override Task<Empty> CacheFile(CacheFileRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.cacheFileRequestProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> ExecuteStep(ExecuteStepRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.executeStepProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> ExecutionEnding(ExecutionEndingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.executionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> ExecutionStarting(ExecutionStartingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.executionStartingProcessor.Process(request));
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
            return Task.FromResult(this.stepNameRequestProcessor.Process(request));
        }

        public override Task<StepNamesResponse> GetStepNames(StepNamesRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepNamesRequestProcessor.Process(request));
        }

        public override Task<StepPositionsResponse> GetStepPositions(StepPositionsRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(this.stepPositionsRequestProcessor.Process(request));
        }

        public override Task<FileDiff> ImplementStub(StubImplementationCodeRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stubImplementationCodeRequestProcessor.Process(request));
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
            return Task.FromResult(this.refactorRequestProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> ScenarioDataStoreInit(Empty request, ServerCallContext context)
        {
            return Task.FromResult(this.scenarioDataStoreInitProcessor.Process());
        }

        public override Task<ExecutionStatusResponse> ScenarioExecutionEnding(ScenarioExecutionEndingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.scenarioExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> ScenarioExecutionStarting(ScenarioExecutionStartingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.scenarioExecutionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> SpecDataStoreInit(Empty request, ServerCallContext context)
        {
            return Task.FromResult(this.specDataStoreInitProcessor.Process());
        }

        public override Task<ExecutionStatusResponse> SpecExecutionEnding(SpecExecutionEndingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.specExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> SpecExecutionStarting(SpecExecutionStartingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.specExecutionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StepExecutionEnding(StepExecutionEndingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StepExecutionStarting(StepExecutionStartingRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepExecutionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> SuiteDataStoreInit(Empty request, ServerCallContext context)
        {
            var activatorWrapper = new ActivatorWrapper();
            var reflectionWrapper = new ReflectionWrapper();
            var assemblies = new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies();
            var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(), assemblies, reflectionWrapper);
            _stepRegistry = assemblyLoader.GetStepRegistry();
            var tableFormatter = new TableFormatter(assemblyLoader, activatorWrapper);
            var classInstanceManager = assemblyLoader.GetClassInstanceManager(activatorWrapper);
            InitializeExecutionMessageHandlers(reflectionWrapper, assemblyLoader, activatorWrapper, tableFormatter, classInstanceManager);
            return Task.FromResult(this.suiteDataStoreInitProcessor.Process());
        }

        public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepValidateRequestProcessor.Process(request));
        }
    }
}
