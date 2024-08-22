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
using Gauge.Dotnet.Executor;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;
using Microsoft.Extensions.Logging.Console;
using static Gauge.Dotnet.Constants;

namespace Gauge.Dotnet;

internal static class Program
{
    private readonly static string STREAMS_COUNT_ENV = "GAUGE_PARALLEL_STREAMS_COUNT";
    private readonly static string ENABLE_MULTITHREADING_ENV = "enable_multithreading";

    private static IConfiguration Configuration { get; set; }

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
            Configuration = builder.Configuration;
            builder.Logging.SetupLogging();
            builder.WebHost.ConfigureKestrel(opts =>
            {
                opts.Listen(IPAddress.Parse("127.0.0.1"), 0);
            });
            builder.Services.ConfigureServices();
            var app = builder.Build();

            Environment.CurrentDirectory = Utils.GaugeProjectRoot;
            var buildSucceeded = TryBuild();
            if (!buildSucceeded && !app.Configuration.GetValue(IgnoreBuildFailures, false))
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
                Logger.Debug("Using ExecutableRunnerServiceHandler");
                app.MapGrpcService<ExecutableRunnerServiceHandler>();
            }
            else
            {
                Logger.Debug("Using AuthoringRunnerServiceHandler");
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


    private static bool TryBuild()
    {
        var buildProjectBuilder = new GaugeProjectBuilder();
        var customBuildPath = Utils.TryReadEnvValue("GAUGE_CUSTOM_BUILD_PATH");
        if (!string.IsNullOrEmpty(customBuildPath))
            return true;

        try
        {
            return buildProjectBuilder.BuildTargetGaugeProject();
        }
        catch (NotAValidGaugeProjectException)
        {
            Logger.Fatal($"Cannot locate a Project File in {Utils.GaugeProjectRoot}");
            return false;
        }
        catch (Exception ex)
        {
            if (!Configuration.GetValue(IgnoreBuildFailures, false))
                Logger.Fatal($"Unable to build Project in {Utils.GaugeProjectRoot}\n{ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    private static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddGrpc();

        var assemblyPath = new AssemblyLocater(new DirectoryWrapper()).GetTestAssembly();
        Logger.Debug($"Loading assembly from : {assemblyPath}");
        services.AddGrpc();
        services.AddSingleton<IReflectionWrapper, ReflectionWrapper>();
        services.AddSingleton<IActivatorWrapper, ActivatorWrapper>();
        services.AddSingleton(new ExecutorPool(GetNoOfStreams(), IsMultithreading()));
        services.AddSingleton<IGaugeLoadContext>((sp) =>
        {
            var isDaemon = string.Compare(Environment.GetEnvironmentVariable("IS_DAEMON"), "true", true) == 0;
            return isDaemon ? new LockFreeGaugeLoadContext(assemblyPath) : new GaugeLoadContext(assemblyPath);
        });
        services.AddSingleton<IAssemblyLoader>((sp) => new AssemblyLoader(assemblyPath, sp.GetRequiredService<IGaugeLoadContext>(),
            sp.GetRequiredService<IReflectionWrapper>(), sp.GetRequiredService<IActivatorWrapper>(), sp.GetRequiredService<IStepRegistry>()));
        services.AddSingleton<IDirectoryWrapper, DirectoryWrapper>();
        services.AddSingleton<IStaticLoader, StaticLoader>();
        services.AddSingleton<IAttributesLoader, AttributesLoader>();
        services.AddSingleton<IStepRegistry>(s => s.GetRequiredService<IStaticLoader>().GetStepRegistry());

        return services;
    }

    private static int GetNoOfStreams()
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

    private static bool IsMultithreading()
    {
        var multithreaded = Environment.GetEnvironmentVariable(ENABLE_MULTITHREADING_ENV);
        if (String.IsNullOrEmpty(multithreaded))
            return false;
        return Boolean.Parse(multithreaded);
    }
}