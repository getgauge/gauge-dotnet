/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
using System;
using System.IO;

namespace Gauge.CSharp.Core
{
    public class Utils
    {
        private const string GaugePortEnv = "GAUGE_INTERNAL_PORT";
        private const string GaugeApiPortEnv = "GAUGE_API_PORT";
        private const string GaugeProjectRootEnv = "GAUGE_PROJECT_ROOT";
        private const string GaugeCustomBuildPath = "GAUGE_CUSTOM_BUILD_PATH";


        public static int GaugePort => Convert.ToInt32(ReadEnvValue(GaugePortEnv));

        public static string GaugeProjectRoot => ReadEnvValue(GaugeProjectRootEnv);

        public static int GaugeApiPort => Convert.ToInt32(ReadEnvValue(GaugeApiPortEnv));

        public static string ReadEnvValue(string env)
        {
            var envValue = TryReadEnvValue(env);
            if (envValue == null)
                throw new Exception(env + " is not set");
            return envValue;
        }

        public static string TryReadEnvValue(string env)
        {
            if (env == null)
                throw new ArgumentNullException("env");

            var envValue = Environment.GetEnvironmentVariable(env.ToUpper());
            if (string.IsNullOrEmpty(envValue))
            {
                envValue = Environment.GetEnvironmentVariable(env.ToLower());
                if (string.IsNullOrEmpty(envValue)) return null;
            }
            return envValue;
        }

        public static string GetGaugeBinDir()
        {
            var customBuildPath = TryReadEnvValue(GaugeCustomBuildPath);
            if (string.IsNullOrEmpty(customBuildPath))
                return Path.Combine(GaugeProjectRoot, "gauge_bin");
            try
            {
                return IsAbsoluteUrl(customBuildPath)
                    ? customBuildPath
                    : Path.Combine(GaugeProjectRoot, customBuildPath);
            }
            catch (Exception)
            {
                return Path.Combine(GaugeProjectRoot, "gauge_bin");
            }
        }

        public static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }
    }
}