// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using Gauge.CSharp.Core;
using NLog;

namespace Gauge.CSharp.Runner
{
    public class GaugeProjectBuilder : IGaugeProjectBuilder
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool BuildTargetGaugeProject()
        {
            var gaugeBinDir = Utils.GetGaugeBinDir();
            var csprojEnvVariable = Utils.TryReadEnvValue("GAUGE_CSHARP_PROJECT_FILE");
            try
            {
                RunDotnetCommand($"publish --configuration=release --output={gaugeBinDir} {csprojEnvVariable}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "C# Project build failed {0}", ex.Message);
                return false;
            }
            return true;
        }

        private static void RunDotnetCommand(string args)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Utils.GaugeProjectRoot,
                FileName = "dotnet",
                Arguments = args
            };
            var buildProcess = new Process { EnableRaisingEvents = true, StartInfo = startInfo };
            buildProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Logger.Info(e.Data);
            };
            buildProcess.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Logger.Error(e.Data);
            };
            buildProcess.Start();
            buildProcess.WaitForExit();
        }
    }
}