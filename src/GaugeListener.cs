/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Executor;
using Gauge.Dotnet.Wrappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Linq;
using Gauge.Dotnet.Models;
using Microsoft.Extensions.Logging;

namespace Gauge.Dotnet
{
    public class GaugeListener
    {
        private readonly string STREAMS_COUNT_ENV = "GAUGE_PARALLEL_STREAMS_COUNT";
        private readonly string ENABLE_MULTITHREADING_ENV = "enable_multithreading";
        public GaugeListener(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            var assemblyPath = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
            Logger.Debug($"Loading assembly from : {assemblyPath}");
            services.AddGrpc();
            services.AddLogging(logConfig => {
                if (Utils.TryReadEnvValue("GAUGE_LOG_LEVEL") == "DEBUG")
                {
                    logConfig.AddFilter("Microsoft", LogLevel.None);
                }
            });
            services.AddSingleton<IReflectionWrapper, ReflectionWrapper>();
            services.AddSingleton<IActivatorWrapper, ActivatorWrapper>();
            services.AddSingleton<ExecutorPool>(new ExecutorPool(GetNoOfStreams(), IsMultithreading()));
            services.AddSingleton<IGaugeLoadContext>((sp) => {
                var isDaemon = string.Compare(Environment.GetEnvironmentVariable("IS_DAEMON"), "true", true) == 0;
                return isDaemon ? new LockFreeGaugeLoadContext(assemblyPath) : new GaugeLoadContext(assemblyPath);
            });
            services.AddSingleton<AssemblyPath>(s => assemblyPath);
            services.AddSingleton<IAssemblyLoader, AssemblyLoader>();
            services.AddSingleton<IDirectoryWrapper, DirectoryWrapper>();
            services.AddSingleton<IStaticLoader, StaticLoader>();
            services.AddSingleton<IAttributesLoader, AttributesLoader>();
            services.AddSingleton<IStepRegistry>(s => s.GetRequiredService<IStaticLoader>().GetStepRegistry());

            if(Configuration.GetValue<string>("ReflectionScanAssemblies") == "True")
            {
                Logger.Debug("Using ExecutableRunnerServiceHandler");
                services.AddSingleton<Gauge.Messages.Runner.RunnerBase, ExecutableRunnerServiceHandler>();
            }
            else
            {
                Logger.Debug("Using AuthoringRunnerServiceHandler");
                services.AddSingleton<Gauge.Messages.Runner.RunnerBase, AuthoringRunnerServiceHandler>();
            }
        }

        public virtual void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            app.UseRouting();
            lifetime.ApplicationStarted.Register(() => {
                var ports = app.ServerFeatures
                    .Get<IServerAddressesFeature>().Addresses
                    .Select(x => new Uri(x).Port).Distinct();
                foreach(var port in ports)
                {
                    Console.WriteLine($"Listening on port:{port}");
                }        
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<Gauge.Messages.Runner.RunnerBase>();
            });
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
            var multithreaded = Environment.GetEnvironmentVariable(ENABLE_MULTITHREADING_ENV);
            if (String.IsNullOrEmpty(multithreaded))
                return false;
            return Boolean.Parse(multithreaded);
        }

    }
}