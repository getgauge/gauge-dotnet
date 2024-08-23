/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Diagnostics;
using System.Runtime.InteropServices;
using Gauge.Dotnet.Exceptions;
using Gauge.Dotnet.Extensions;

namespace Gauge.Dotnet;

public class GaugeProjectBuilder : IGaugeProjectBuilder
{
    private readonly IConfiguration _config;

    public GaugeProjectBuilder(IConfiguration config)
    {
        _config = config;
    }

    public bool BuildTargetGaugeProject()
    {
        var customBuildPath = _config.GetGaugeCustomBuildPath();
        if (!string.IsNullOrEmpty(customBuildPath))
            return true;

        try
        {
            var gaugeBinDir = _config.GetGaugeBinDir();
            var csprojEnvVariable = _config.GetGaugeCSharpProjectFile();
            var additionalBuildArgs = _config.GetGaugeCSharpBuildArgs();
            var runtime = _config.GetGaugeCSharpRuntime();

            if (string.IsNullOrEmpty(runtime))
            {
                runtime = $"{GetOS()}-{GetArch()}";
            }

            var configurationEnvVariable = _config.GetGaugeCSharpConfig();
            var commandArgs = $"publish --runtime={runtime} --no-self-contained --configuration={configurationEnvVariable} --output=\"{gaugeBinDir}\" {additionalBuildArgs}";

            if (!string.IsNullOrEmpty(csprojEnvVariable))
            {
                commandArgs = $"{commandArgs} \"{csprojEnvVariable}\"";
            }

            var logLevel = _config.GetGaugeLogLevel();

            if (string.Compare(logLevel, "DEBUG", true) != 0)
            {
                commandArgs = $"{commandArgs} --verbosity=quiet";
            }

            if (RunDotnetCommand(commandArgs) != 0)
            {
                throw new Exception($"dotnet Project build failed.\nRan 'dotnet {commandArgs}'");
            }

            return true;
        }
        catch (NotAValidGaugeProjectException)
        {
            Logger.Fatal($"Cannot locate a Project File in {_config.GetGaugeProjectRoot()}");
            return false;
        }
        catch (Exception ex)
        {
            if (!_config.IgnoreBuildFailures())
                Logger.Fatal($"Unable to build Project in {_config.GetGaugeProjectRoot()}\n{ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    private int RunDotnetCommand(string args)
    {
        var startInfo = new ProcessStartInfo
        {
            WorkingDirectory = _config.GetGaugeProjectRoot(),
            FileName = "dotnet",
            Arguments = args
        };
        var buildProcess = new Process { EnableRaisingEvents = true, StartInfo = startInfo };
        buildProcess.OutputDataReceived += (sender, e) => { Logger.Debug(e.Data); };
        buildProcess.ErrorDataReceived += (sender, e) => { Logger.Error(e.Data); };
        buildProcess.Start();
        buildProcess.WaitForExit();
        return buildProcess.ExitCode;
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