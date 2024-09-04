/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Diagnostics;
using System.Net;
using System.Reflection;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Exceptions;
using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Microsoft.Extensions.Logging.Console;

namespace Gauge.Dotnet;

internal static class Program
{
    [STAThread]
    [DebuggerHidden]
    private static async Task Main(string[] args)
    {
        if (args.Length == 0 || args[0] != "--start")
        {
            Console.WriteLine("usage: {0} --start", AppDomain.CurrentDomain.FriendlyName);
            Environment.Exit(1);
        }

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.SetupConfiguration();
            builder.Logging.SetupLogging();
            builder.WebHost.ConfigureKestrel(opts =>
            {
                opts.Listen(IPAddress.Parse("127.0.0.1"), 0);
            });
            builder.Services.ConfigureServices(builder.Configuration);
            var app = builder.Build();

            Environment.CurrentDirectory = app.Configuration.GetGaugeProjectRoot();
            var buildSucceeded = app.Services.GetRequiredService<IGaugeProjectBuilder>().BuildTargetGaugeProject();
            if (!buildSucceeded && !app.Configuration.IgnoreBuildFailures())
            {
                return;
            }

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                var ports = app.Urls.Select(x => new Uri(x).Port).Distinct();
                foreach (var port in ports)
                {
                    Console.WriteLine($"Listening on port:{port}");
                }
            });

            if (buildSucceeded)
            {
                app.MapGrpcService<ExecutableRunnerServiceHandler>();
            }
            else
            {
                app.MapGrpcService<AuthoringRunnerServiceHandler>();
            }
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            await app.RunAsync();
        }
        catch (TargetInvocationException e)
        {
            if (!(e.InnerException is GaugeLibVersionMismatchException))
                throw;
            Logger.Fatal(e.InnerException.Message);
        }
    }

    private static IConfigurationBuilder SetupConfiguration(this IConfigurationBuilder builder) =>
        builder.AddJsonFile($"{Utils.GaugeProjectRoot}/appsettings.json", true).AddEnvironmentVariables();

    public static ILoggingBuilder SetupLogging(this ILoggingBuilder builder) =>
        builder.AddConsole().AddConsoleFormatter<GaugeLoggingFormatter, ConsoleFormatterOptions>();


    private static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddGrpc();
        services.AddTransient<IGaugeProjectBuilder, GaugeProjectBuilder>();
        services.AddTransient<IAssemblyLocater, AssemblyLocater>();
        services.AddSingleton<IReflectionWrapper, ReflectionWrapper>();
        services.AddSingleton<IActivatorWrapper, ActivatorWrapper>();
        services.AddSingleton<IGaugeLoadContext>((sp) =>
        {
            return config.IsDaemon() ?
                new LockFreeGaugeLoadContext(sp.GetRequiredService<IAssemblyLocater>()) :
                new GaugeLoadContext(sp.GetRequiredService<IAssemblyLocater>());
        });
        services.AddSingleton<IAssemblyLoader, AssemblyLoader>();
        services.AddSingleton<IDirectoryWrapper, DirectoryWrapper>();
        services.AddSingleton<IStaticLoader, StaticLoader>();
        services.AddSingleton<IAttributesLoader, AttributesLoader>();
        services.AddSingleton<IHookRegistry, HookRegistry>();
        services.AddSingleton<IStepRegistry>(s => s.GetRequiredService<IStaticLoader>().GetStepRegistry());
        services.AddSingleton<IExecutor, Executor>();
        services.AddSingleton<IStepExecutor, StepExecutor>();
        services.AddSingleton<IHookExecutor, HookExecutor>();
        services.AddSingleton<ITableFormatter, TableFormatter>();
        services.AddSingleton<IExecutionOrchestrator, ExecutionOrchestrator>();
        services.AddSingleton<IExecutionInfoMapper, ExecutionInfoMapper>();
        services.AddTransient<IGaugeProcessor<StepValidateRequest, StepValidateResponse>, StepValidationProcessor>();
        services.AddTransient<IGaugeProcessor<CacheFileRequest, Empty>, CacheFileProcessor>();
        services.AddTransient<IGaugeProcessor<Empty, ImplementationFileGlobPatternResponse>, ImplementationFileGlobPatterProcessor>();
        services.AddTransient<IGaugeProcessor<Empty, ImplementationFileListResponse>, ImplementationFileListProcessor>();
        services.AddTransient<IGaugeProcessor<StepNameRequest, StepNameResponse>, StepNameProcessor>();
        services.AddTransient<IGaugeProcessor<StepNamesRequest, StepNamesResponse>, StepNamesProcessor>();
        services.AddTransient<IGaugeProcessor<StepPositionsRequest, StepPositionsResponse>, StepPositionsProcessor>();
        services.AddTransient<IGaugeProcessor<StubImplementationCodeRequest, FileDiff>, StubImplementationCodeProcessor>();
        services.AddTransient<IGaugeProcessor<RefactorRequest, RefactorResponse>, RefactorProcessor>();
        services.AddTransient<IGaugeProcessor<SuiteDataStoreInitRequest, ExecutionStatusResponse>, SuiteDataStoreInitProcessor>();
        services.AddTransient<IGaugeProcessor<SuiteDataStoreInitRequest, ExecutionStatusResponse>, SuiteDataStoreInitProcessor>();
        services.AddTransient<IGaugeProcessor<ExecuteStepRequest, ExecutionStatusResponse>, ExecuteStepProcessor>();
        services.AddTransient<IGaugeProcessor<ExecutionEndingRequest, ExecutionStatusResponse>, ExecutionEndingProcessor>();
        services.AddTransient<IGaugeProcessor<ScenarioExecutionEndingRequest, ExecutionStatusResponse>, ScenarioExecutionEndingProcessor>();
        services.AddTransient<IGaugeProcessor<SpecExecutionEndingRequest, ExecutionStatusResponse>, SpecExecutionEndingProcessor>();
        services.AddTransient<IGaugeProcessor<StepExecutionEndingRequest, ExecutionStatusResponse>, StepExecutionEndingProcessor>();
        services.AddTransient<IGaugeProcessor<ScenarioDataStoreInitRequest, ExecutionStatusResponse>, ScenarioDataStoreInitProcessor>();
        services.AddTransient<IGaugeProcessor<SpecDataStoreInitRequest, ExecutionStatusResponse>, SpecDataStoreInitProcessor>();
        services.AddTransient<IGaugeProcessor<ExecutionStartingRequest, ExecutionStatusResponse>, ExecutionStartingProcessor>();
        services.AddTransient<IGaugeProcessor<ScenarioExecutionStartingRequest, ExecutionStatusResponse>, ScenarioExecutionStartingProcessor>();
        services.AddTransient<IGaugeProcessor<SpecExecutionStartingRequest, ExecutionStatusResponse>, SpecExecutionStartingProcessor>();
        services.AddTransient<IGaugeProcessor<StepExecutionStartingRequest, ExecutionStatusResponse>, StepExecutionStartingProcessor>();
        services.AddTransient<IGaugeProcessor<ConceptExecutionStartingRequest, ExecutionStatusResponse>, ConceptExecutionStartingProcessor>();
        services.AddTransient<IGaugeProcessor<ConceptExecutionEndingRequest, ExecutionStatusResponse>, ConceptExecutionEndingProcessor>();

        return services;
    }
}