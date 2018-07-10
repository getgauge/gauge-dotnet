// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-Dotnet.
//
// Gauge-Dotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-Dotnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-Dotnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using Gauge.CSharp.Core;
using NLog;

namespace Gauge.Dotnet
{
    public class GaugeProjectBuilder : IGaugeProjectBuilder
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool BuildTargetGaugeProject()
        {
            var gaugeBinDir = Utils.GetGaugeBinDir();
            var csprojEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_FILE");
            string configurationEnvVariable = ReadBuildConfiguration();
            var commandArgs = $"publish --configuration={configurationEnvVariable} --output=\"{gaugeBinDir}\"";
            if (!string.IsNullOrEmpty(csprojEnvVariable)) commandArgs = $"{commandArgs} \"{csprojEnvVariable}\"";
            try
            {
                var logLevel = Utils.TryReadEnvValue("GAUGE_LOG_LEVEL");
                if (string.Compare(logLevel, "DEBUG", true) != 0) commandArgs = $"{commandArgs} --verbosity=quiet";
                RunDotnetCommand(commandArgs);
            }
            catch (Exception ex)
            {
                throw new Exception($"dotnet Project build failed.\nRan 'dotnet {commandArgs}'", ex);
            }

            return true;
        }

        public static void RunDotnetCommand(string args)
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
        }

        private static string ReadBuildConfiguration()
        {
            var configurationEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_CONFIG");
            if (string.IsNullOrEmpty(configurationEnvVariable))
            {
                configurationEnvVariable = "release";
            }
            return configurationEnvVariable;
        }
    }
}