/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Diagnostics;
using System.Reflection;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Exceptions;

namespace Gauge.Dotnet
{
    public class StartCommand : IGaugeCommand
    {
        private readonly Func<IGaugeListener> _gaugeListener;
        private readonly Func<IGaugeProjectBuilder> _projectBuilder;

        public StartCommand(Func<IGaugeListener> gaugeListener, Func<IGaugeProjectBuilder> projectBuilder)
        {
            Environment.CurrentDirectory = Utils.GaugeProjectRoot;
            _gaugeListener = gaugeListener;
            _projectBuilder = projectBuilder;
        }

        [DebuggerHidden]
        public void Execute()
        {
            if (!TryBuild() && !this.ShouldContinueBuildFailure())
            {
                return;
            }
            try
            {
                _gaugeListener.Invoke().StartServer(!this.ShouldContinueBuildFailure());
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
                return _projectBuilder.Invoke().BuildTargetGaugeProject();
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