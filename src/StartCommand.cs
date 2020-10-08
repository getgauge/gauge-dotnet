/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace Gauge.Dotnet
{
    public class StartCommand : IGaugeCommand
    {
        private readonly Lazy<IGaugeProjectBuilder> _projectBuilder;

        public StartCommand(Lazy<IGaugeProjectBuilder> projectBuilder)
        {
            Environment.CurrentDirectory = Utils.GaugeProjectRoot;
            _projectBuilder = projectBuilder;
        }

        [DebuggerHidden]
        public void Execute()
        {
            var buildSucceeded = TryBuild();
            if (!buildSucceeded && !this.ShouldContinueBuildFailure())
            {
                return;
            }
            try
            {
                var builder = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(wb => {
                        wb.UseShutdownTimeout(TimeSpan.FromMilliseconds(0));
                        wb.UseStartup<GaugeListener>();
                        wb.UseSetting("ReflectionScanAssemblies", buildSucceeded.ToString());
                        wb.ConfigureKestrel(options => 
                            options.Listen(IPAddress.Parse("127.0.0.1"), 0, lo => lo.Protocols = HttpProtocols.Http2));
                    });

                using(var host = builder.Build()){
                    host.Run();
                    Environment.Exit(0);
                };
            }
            catch (TargetInvocationException e)
            {
                if (!(e.InnerException is GaugeLibVersionMismatchException))
                    throw;
                Logger.Fatal(e.InnerException.Message);
            }
        }

        private bool ShouldContinueBuildFailure()
        {
            var continueOnFailure = Utils.TryReadEnvValue("GAUGE_IGNORE_RUNNER_BUILD_FAILURES");
            return !string.IsNullOrEmpty(continueOnFailure) && continueOnFailure == "true";
        }

        private bool TryBuild()
        {
            var customBuildPath = Utils.TryReadEnvValue("GAUGE_CUSTOM_BUILD_PATH");
            if (!string.IsNullOrEmpty(customBuildPath))
                return true;

            try
            {
                return _projectBuilder.Value.BuildTargetGaugeProject();
            }
            catch (NotAValidGaugeProjectException)
            {
                Logger.Fatal($"Cannot locate a Project File in {Utils.GaugeProjectRoot}");
                return false;
            }
            catch (Exception ex)
            {
                if (!this.ShouldContinueBuildFailure())
                    Logger.Fatal($"Unable to build Project in {Utils.GaugeProjectRoot}\n{ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
    }
}