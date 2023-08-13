/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Gauge.CSharp.Core;

namespace Gauge.Dotnet
{
    public class GaugeProjectBuilder : IGaugeProjectBuilder
    {
        public bool BuildTargetGaugeProject()
        {
            var gaugeBinDir = Utils.GetGaugeBinDir();
            var csprojEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_FILE");
            var additionalBuildArgs = Utils.TryReadEnvValue("GAUGE_DOTNET_BUILD_ARGS");
            var runtime = Utils.TryReadEnvValue("GAUGE_DOTNET_RUNTIME");

            if (string.IsNullOrEmpty(runtime))
            {
                runtime = $"{GetOS()}-{GetArch()}";
            }

            var configurationEnvVariable = ReadBuildConfiguration();
            var commandArgs = $"publish --runtime={runtime} --no-self-contained --configuration={configurationEnvVariable} --output=\"{gaugeBinDir}\" {additionalBuildArgs}";

            if (!string.IsNullOrEmpty(csprojEnvVariable))
            {
                commandArgs = $"{commandArgs} \"{csprojEnvVariable}\"";
            }

            var logLevel = Utils.TryReadEnvValue("GAUGE_LOG_LEVEL");

            if (string.Compare(logLevel, "DEBUG", true) != 0)
            {
                commandArgs = $"{commandArgs} --verbosity=quiet";
            }

            if(RunDotnetCommand(commandArgs) !=0)
            {
                throw new Exception($"dotnet Project build failed.\nRan 'dotnet {commandArgs}'");
            }

            return true;
        }

        private static int RunDotnetCommand(string args)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Utils.GaugeProjectRoot,
                FileName = "dotnet",
                Arguments = args
            };
            var buildProcess = new Process {EnableRaisingEvents = true, StartInfo = startInfo};
            buildProcess.OutputDataReceived += (sender, e) => { Logger.Debug(e.Data); };
            buildProcess.ErrorDataReceived += (sender, e) => { Logger.Error(e.Data); };
            buildProcess.Start();
            buildProcess.WaitForExit();
            return buildProcess.ExitCode;
        }

        private static string ReadBuildConfiguration()
        {
            var configurationEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_CONFIG");
            if (string.IsNullOrEmpty(configurationEnvVariable)) configurationEnvVariable = "release";

            return configurationEnvVariable;
        }

        private static string GetOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "win";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "osx";
            }
            return null;
        }

        private static string GetArch()
        {
            return RuntimeInformation.ProcessArchitecture.ToString().ToLower();
        }

    }
}