/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Diagnostics;
using Gauge.CSharp.Core;

namespace Gauge.Dotnet
{
    public class GaugeProjectBuilder : IGaugeProjectBuilder
    {

        public bool BuildTargetGaugeProject()
        {
            var gaugeBinDir = Utils.GetGaugeBinDir();
            var csprojEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_FILE");
            var configurationEnvVariable = ReadBuildConfiguration();
            var commandArgs = $"publish --configuration={configurationEnvVariable} --output=\"{gaugeBinDir}\"";
            if (!string.IsNullOrEmpty(csprojEnvVariable))
            {
                commandArgs = $"{commandArgs} \"{csprojEnvVariable}\"";
            }
            var logLevel = Utils.TryReadEnvValue("GAUGE_LOG_LEVEL");
            if (string.Compare(logLevel, "DEBUG", true) != 0) commandArgs = $"{commandArgs} --verbosity=quiet";
            if(RunDotnetCommand(commandArgs) !=0)
            {
                throw new Exception($"dotnet Project build failed.\nRan 'dotnet {commandArgs}'");
            }
            return true;
        }

        public static int RunDotnetCommand(string args)
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
    }
}