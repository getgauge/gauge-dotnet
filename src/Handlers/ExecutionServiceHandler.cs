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
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet.Handlers
{
    internal class ExecutionServiceHandler : Execution.ExecutionBase
    {
        private IStepRegistry _stepRegistry;
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

        public ExecutionServiceHandler(IStaticLoader loader)
        {
            _stepRegistry = loader.GetStepRegistry();
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

    }
}