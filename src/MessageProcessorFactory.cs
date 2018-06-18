// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet
{
    public class MessageProcessorFactory
    {
        private readonly Dictionary<Message.Types.MessageType, IMessageProcessor> _messageProcessorsDictionary;
        private IStepRegistry _stepRegistry;

        public MessageProcessorFactory(IStepRegistry registry)
        {
            _stepRegistry = registry;
            _messageProcessorsDictionary = InitializeMessageHandlers();
        }

        public IMessageProcessor GetProcessor(Message.Types.MessageType messageType, bool scan = false)
        {
            if (scan)
            {
                var reflectionWrapper = new ReflectionWrapper();
                var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(),
                    new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies(),
                    reflectionWrapper);
                _stepRegistry = assemblyLoader.GetStepRegistry();
                var activatorWrapper = new ActivatorWrapper();
                var tableFormatter = new TableFormatter(assemblyLoader, activatorWrapper);
                var sandbox = new Sandbox(assemblyLoader, new HookRegistry(assemblyLoader), activatorWrapper,
                    reflectionWrapper);
                InitializeExecutionMessageHandlers(reflectionWrapper, assemblyLoader, activatorWrapper, tableFormatter,
                    sandbox);
            }

            return !_messageProcessorsDictionary.ContainsKey(messageType)
                ? new DefaultProcessor()
                : _messageProcessorsDictionary[messageType];
        }

        public void InitializeExecutionMessageHandlers(IReflectionWrapper reflectionWrapper,
            IAssemblyLoader assemblyLoader, IActivatorWrapper activatorWrapper, ITableFormatter tableFormatter,
            ISandbox sandbox)
        {
            var methodExecutor = new MethodExecutor(sandbox);
            var handlers = new Dictionary<Message.Types.MessageType, IMessageProcessor>
            {
                {
                    Message.Types.MessageType.ExecutionStarting,
                    new ExecutionStartingProcessor(methodExecutor, assemblyLoader, reflectionWrapper)
                },
                {
                    Message.Types.MessageType.ExecutionEnding,
                    new ExecutionEndingProcessor(methodExecutor, assemblyLoader, reflectionWrapper)
                },
                {
                    Message.Types.MessageType.SpecExecutionStarting,
                    new SpecExecutionStartingProcessor(methodExecutor, sandbox, assemblyLoader, reflectionWrapper)
                },
                {
                    Message.Types.MessageType.SpecExecutionEnding,
                    new SpecExecutionEndingProcessor(methodExecutor, sandbox, assemblyLoader, reflectionWrapper)
                },
                {
                    Message.Types.MessageType.ScenarioExecutionStarting,
                    new ScenarioExecutionStartingProcessor(methodExecutor, sandbox, assemblyLoader, reflectionWrapper)
                },
                {
                    Message.Types.MessageType.ScenarioExecutionEnding,
                    new ScenarioExecutionEndingProcessor(methodExecutor, sandbox, assemblyLoader, reflectionWrapper)
                },
                {
                    Message.Types.MessageType.StepExecutionStarting,
                    new StepExecutionStartingProcessor(methodExecutor, assemblyLoader, reflectionWrapper)
                },
                {
                    Message.Types.MessageType.StepExecutionEnding,
                    new StepExecutionEndingProcessor(methodExecutor, assemblyLoader, reflectionWrapper)
                },
                {
                    Message.Types.MessageType.ExecuteStep,
                    new ExecuteStepProcessor(_stepRegistry, methodExecutor, tableFormatter)
                },
                {Message.Types.MessageType.KillProcessRequest, new KillProcessProcessor()},
                {Message.Types.MessageType.ScenarioDataStoreInit, new ScenarioDataStoreInitProcessor(assemblyLoader)},
                {Message.Types.MessageType.SpecDataStoreInit, new SpecDataStoreInitProcessor(assemblyLoader)},
                {Message.Types.MessageType.SuiteDataStoreInit, new SuiteDataStoreInitProcessor(assemblyLoader)}
            };
            foreach (var handler in handlers) _messageProcessorsDictionary.Add(handler.Key, handler.Value);
        }


        private Dictionary<Message.Types.MessageType, IMessageProcessor> InitializeMessageHandlers()
        {
            return new Dictionary<Message.Types.MessageType, IMessageProcessor>
            {
                {Message.Types.MessageType.StepNamesRequest, new StepNamesProcessor(_stepRegistry)},
                {Message.Types.MessageType.StepValidateRequest, new StepValidationProcessor(_stepRegistry)},
                {Message.Types.MessageType.StepNameRequest, new StepNameProcessor(_stepRegistry)},
                {Message.Types.MessageType.RefactorRequest, new RefactorProcessor(_stepRegistry)}
            };
        }
    }
}