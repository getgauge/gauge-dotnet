// Copyright 2015 ThoughtWorks, Inc.
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

using Gauge.CSharp.Core;
using Gauge.CSharp.Runner.Models;
using Gauge.CSharp.Runner.Processors;
using Gauge.CSharp.Runner.Wrappers;

namespace Gauge.CSharp.Runner
{
    public class GaugeCommandFactory
    {
        public static IGaugeCommand GetExecutor(string phase)
        {
            switch (phase)
            {
                case "--init":
                    return new SetupCommand();
                default:
                    return new StartCommand(() =>
                    {
                        using (var apiConnection = new GaugeApiConnection(new TcpClientWrapper(Utils.GaugeApiPort)))
                        {
                            var assemblyLoader = new AssemblyLoader();
                            var activatorWrapper = new ActivatorWrapper();
                            var sandBox = new Sandbox(assemblyLoader, new HookRegistry(assemblyLoader), activatorWrapper, new ReflectionWrapper());
                            var methodScanner = new MethodScanner(apiConnection, sandBox);
                            var messageProcessorFactory = new MessageProcessorFactory(methodScanner, sandBox, assemblyLoader, activatorWrapper, new TableFormatter(assemblyLoader, activatorWrapper));
                            return new GaugeListener(messageProcessorFactory);
                        }
                    },
                    () => new GaugeProjectBuilder());
            }
        }
    }
}