/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Linq;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet
{
    public class GaugeListener : IGaugeListener
    {
        private readonly IStaticLoader _staticLoader;

        public GaugeListener(IStaticLoader loader)
        {
            this._staticLoader = loader;
        }

        public void StartServer(bool scanAssemblies)
        {
            var server = new Server();
            if (scanAssemblies)
            {
                var assemblyPath = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
                var reflectionWrapper = new ReflectionWrapper();
                var activatorWrapper = new ActivatorWrapper();
                Logger.Debug($"Loading assembly from : {assemblyPath}");
                if (string.Compare(Environment.GetEnvironmentVariable("IS_DAEMON"), "true", true) == 0)
                {
                    using (var gaugeLoadContext = new DisposableGaugeLoadContext(assemblyPath))
                    {
                        var assemblyLoader = new AssemblyLoader(assemblyPath, gaugeLoadContext, reflectionWrapper, activatorWrapper, _staticLoader.GetStepRegistry());
                        var handler = new RunnerServiceHandler(activatorWrapper,reflectionWrapper, assemblyLoader, _staticLoader, server);
                        server.Services.Add(Runner.BindService(handler));
                    }
                } else {
                    var assemblyLoader = new AssemblyLoader(assemblyPath, new GaugeLoadContext(assemblyPath), reflectionWrapper,
                        activatorWrapper, _staticLoader.GetStepRegistry());
                    var handler = new RunnerServiceHandler(activatorWrapper,reflectionWrapper, assemblyLoader, _staticLoader, server);
                    server.Services.Add(Runner.BindService(handler));
                }
            } else {
                var handler = new RunnerServiceHandler(_staticLoader, server);
                server.Services.Add(Runner.BindService(handler));
            }
            var port = server.Ports.Add(new ServerPort("127.0.0.1", 0, ServerCredentials.Insecure));
            server.Start();
            Console.WriteLine("Listening on port:" + port);
            server.ShutdownTask.Wait();
            Environment.Exit(Environment.ExitCode);
        }
    }
}