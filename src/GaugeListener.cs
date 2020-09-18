/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Executor;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet
{
    public class GaugeListener : IGaugeListener
    {
        private readonly string STREAMS_COUNT_ENV = "GAUGE_PARALLEL_STREAMS_COUNT";
        private readonly string ENABLE_MULTITHREADING_ENV = "enable_multithreading";

        private readonly IStaticLoader _staticLoader;

        public GaugeListener(IStaticLoader loader)
        {
            this._staticLoader = loader;
        }

        public void StartServer(bool scanAssemblies)
        {
            var server = new Server();
            var pool = new ExecutorPool(GetNoOfStreams(), IsMultithreading());
            if (scanAssemblies)
            {
                var assemblyPath = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
                var reflectionWrapper = new ReflectionWrapper();
                var activatorWrapper = new ActivatorWrapper();
                Logger.Debug($"Loading assembly from : {assemblyPath}");
                var isDaemon = string.Compare(Environment.GetEnvironmentVariable("IS_DAEMON"), "true", true) == 0;
                var gaugeLoadContext = isDaemon ? new LockFreeGaugeLoadContext(assemblyPath) : new GaugeLoadContext(assemblyPath);
                var assemblyLoader = new AssemblyLoader(assemblyPath, gaugeLoadContext, reflectionWrapper, activatorWrapper, _staticLoader.GetStepRegistry());
                var handler = new RunnerServiceHandler(activatorWrapper, reflectionWrapper, assemblyLoader, _staticLoader, server, pool);
                server.Services.Add(Runner.BindService(handler));
            }
            else
            {
                var handler = new RunnerServiceHandler(_staticLoader, server, pool);
                server.Services.Add(Runner.BindService(handler));
            }
            var port = server.Ports.Add(new ServerPort("127.0.0.1", 0, ServerCredentials.Insecure));
            server.Start();
            Console.WriteLine("Listening on port:" + port);
            server.ShutdownTask.Wait();
            Environment.Exit(Environment.ExitCode);
        }
        private int GetNoOfStreams()
        {
            int numberOfStreams = 1;
            if (IsMultithreading())
            {
                String streamsCount = Utils.TryReadEnvValue(STREAMS_COUNT_ENV);
                try
                {
                    numberOfStreams = int.Parse(streamsCount);
                    Logger.Debug("multithreading enabled, number of threads=" + numberOfStreams);
                }
                catch (Exception e)
                {
                    Logger.Debug("multithreading enabled, but could not read " + STREAMS_COUNT_ENV + " as int. Got " + STREAMS_COUNT_ENV + "=" + streamsCount);
                    Logger.Debug("using numberOfStreams=1, err: " + e.Message);
                }
            }
            return numberOfStreams;
        }

        private bool IsMultithreading()
        {
            var multithreaded = Environment.GetEnvironmentVariable("enable_multithreading");
            if (String.IsNullOrEmpty(multithreaded))
                return false;
            return Boolean.Parse(multithreaded);
        }

    }
}