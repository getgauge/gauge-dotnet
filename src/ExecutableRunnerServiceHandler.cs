/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Threading;
using System.Threading.Tasks;
using Gauge.Dotnet.Executor;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Grpc.Core;
using Microsoft.Extensions.Hosting;

namespace Gauge.Dotnet
{
    internal class ExecutableRunnerServiceHandler : AuthoringRunnerServiceHandler
    {
        private readonly IActivatorWrapper _activatorWrapper;
        private readonly IReflectionWrapper _reflectionWrapper;
        private readonly IAssemblyLoader _assemblyLoader;
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
       public ExecutableRunnerServiceHandler(IActivatorWrapper activationWrapper, IReflectionWrapper reflectionWrapper, 
            IAssemblyLoader assemblyLoader, IStaticLoader loader, ExecutorPool pool, IHostApplicationLifetime lifetime)
            : base(loader, pool, lifetime)
        {
            _activatorWrapper = activationWrapper;
            _reflectionWrapper = reflectionWrapper;
            _assemblyLoader = assemblyLoader;
            _stepRegistry = assemblyLoader.GetStepRegistry();
            InitializeExecutionMessageHandlers();
        }
        public override Task<ExecutionStatusResponse> InitializeSuiteDataStore(SuiteDataStoreInitRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), () => suiteDataStoreInitProcessor.Process());
        }

         public override Task<ExecutionStatusResponse> ExecuteStep(ExecuteStepRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), async () => await executeStepProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishExecution(ExecutionEndingRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), async () => await executionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishScenarioExecution(ScenarioExecutionEndingRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), async () => await scenarioExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishSpecExecution(SpecExecutionEndingRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), async () => await specExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishStepExecution(StepExecutionEndingRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), async () => await stepExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> InitializeScenarioDataStore(ScenarioDataStoreInitRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), () => scenarioDataStoreInitProcessor.Process());
        }

        public override Task<ExecutionStatusResponse> InitializeSpecDataStore(SpecDataStoreInitRequest request, ServerCallContext context)
        {
            try
            {
                return _pool.Execute(GetStream(request.Stream), () => specDataStoreInitProcessor.Process());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(1);
            }
            return null;
        }

        public override Task<ExecutionStatusResponse> StartExecution(ExecutionStartingRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), async () => await executionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StartScenarioExecution(ScenarioExecutionStartingRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), async () => await scenarioExecutionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StartSpecExecution(SpecExecutionStartingRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), async () => await specExecutionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StartStepExecution(StepExecutionStartingRequest request, ServerCallContext context)
        {
            return _pool.Execute(GetStream(request.Stream), async () => await stepExecutionStartingProcessor.Process(request));
        }

        private void InitializeExecutionMessageHandlers()
        {
            var tableFormatter = new TableFormatter(_assemblyLoader, _activatorWrapper);
            var classInstanceManager = new ThreadLocal<object>(() => _assemblyLoader.GetClassInstanceManager());
            var executionInfoMapper = new ExecutionInfoMapper(_assemblyLoader, _activatorWrapper);
            var executionOrchestrator = new ExecutionOrchestrator(_reflectionWrapper, _assemblyLoader,
                classInstanceManager.Value,
                new HookExecutor(_assemblyLoader, _reflectionWrapper, classInstanceManager.Value, executionInfoMapper),
                new StepExecutor(_assemblyLoader, _reflectionWrapper, classInstanceManager.Value));

            executionStartingProcessor = new ExecutionStartingProcessor(executionOrchestrator);
            executionEndingProcessor = new ExecutionEndingProcessor(executionOrchestrator);
            specExecutionStartingProcessor = new SpecExecutionStartingProcessor(executionOrchestrator);
            specExecutionEndingProcessor = new SpecExecutionEndingProcessor(executionOrchestrator);
            scenarioExecutionStartingProcessor = new ScenarioExecutionStartingProcessor(executionOrchestrator);
            scenarioExecutionEndingProcessor = new ScenarioExecutionEndingProcessor(executionOrchestrator);
            stepExecutionStartingProcessor = new StepExecutionStartingProcessor(executionOrchestrator);
            stepExecutionEndingProcessor = new StepExecutionEndingProcessor(executionOrchestrator);
            executeStepProcessor = new ExecuteStepProcessor(_stepRegistry, executionOrchestrator, tableFormatter);
            scenarioDataStoreInitProcessor = new ScenarioDataStoreInitProcessor(_assemblyLoader);
            specDataStoreInitProcessor = new SpecDataStoreInitProcessor(_assemblyLoader);
            suiteDataStoreInitProcessor = new SuiteDataStoreInitProcessor(_assemblyLoader);
        }

        private int GetStream(int stream)
        {
            return _pool.IsMultithreading ? Math.Max(stream, 1) : 1;
        }
    }
}