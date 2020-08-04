/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
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
                var assemblies = new AssemblyLocater(new DirectoryWrapper(), new FileWrapper()).GetAllAssemblies();
                var reflectionWrapper = new ReflectionWrapper();
                var activatorWrapper = new ActivatorWrapper();
                var assemblyLoader = new AssemblyLoader(new AssemblyWrapper(), assemblies, reflectionWrapper, activatorWrapper, _staticLoader.GetStepRegistry());
                var handler = new RunnerServiceHandler(activatorWrapper, reflectionWrapper, assemblyLoader, _staticLoader, server);
                server.Services.Add(Runner.BindService(handler));

            }else {
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