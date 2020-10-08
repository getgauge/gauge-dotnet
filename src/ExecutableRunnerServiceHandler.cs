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
            this._activatorWrapper = activationWrapper;
            this._reflectionWrapper = reflectionWrapper;
            this._assemblyLoader = assemblyLoader;
            _stepRegistry = assemblyLoader.GetStepRegistry();
            InitializeExecutionMessageHandlers();
        }
        public override Task<ExecutionStatusResponse> InitializeSuiteDataStore(SuiteDataStoreInitRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.suiteDataStoreInitProcessor.Process());
        }

         public override Task<ExecutionStatusResponse> ExecuteStep(ExecuteStepRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.executeStepProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishExecution(ExecutionEndingRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.executionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishScenarioExecution(ScenarioExecutionEndingRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.scenarioExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishSpecExecution(SpecExecutionEndingRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.specExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> FinishStepExecution(StepExecutionEndingRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.stepExecutionEndingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> InitializeScenarioDataStore(ScenarioDataStoreInitRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.scenarioDataStoreInitProcessor.Process());
        }

        public override Task<ExecutionStatusResponse> InitializeSpecDataStore(SpecDataStoreInitRequest request, ServerCallContext context)
        {
            try
            {
                return _pool.Execute(getStream(request.Stream), () => this.specDataStoreInitProcessor.Process());
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(1);
            }
            return null;
        }

        public override Task<ExecutionStatusResponse> StartExecution(ExecutionStartingRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.executionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StartScenarioExecution(ScenarioExecutionStartingRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.scenarioExecutionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StartSpecExecution(SpecExecutionStartingRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.specExecutionStartingProcessor.Process(request));
        }

        public override Task<ExecutionStatusResponse> StartStepExecution(StepExecutionStartingRequest request, ServerCallContext context)
        {
            return _pool.Execute(getStream(request.Stream), () => this.stepExecutionStartingProcessor.Process(request));
        }

        private void InitializeExecutionMessageHandlers()
        {
            Console.WriteLine("InitializeExecutionMessageHandlers");
            var tableFormatter = new TableFormatter(this._assemblyLoader, this._activatorWrapper);
            var classInstanceManager = new ThreadLocal<object>(() =>
            {
                return this._assemblyLoader.GetClassInstanceManager();
            });
            var executionInfoMapper = new ExecutionInfoMapper(this._assemblyLoader, this._activatorWrapper);
            var executionOrchestrator = new ExecutionOrchestrator(this._reflectionWrapper, this._assemblyLoader,
                classInstanceManager.Value,
                new HookExecutor(this._assemblyLoader, this._reflectionWrapper, classInstanceManager.Value, executionInfoMapper),
                new StepExecutor(this._assemblyLoader, this._reflectionWrapper, classInstanceManager.Value));

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

        private int getStream(int stream)
        {
            if (!_pool.IsMultithreading)
            {
                return 1;
            }
            return Math.Max(stream, 1);
        }
    }
}