namespace Gauge.Dotnet.Extensions;

public static class ConfigurationExtensions
{
    public static bool IgnoreBuildFailures(this IConfiguration config) =>
        config.GetValue("GAUGE_IGNORE_RUNNER_BUILD_FAILURES", false);

    public static bool IsDaemon(this IConfiguration config) =>
        config.GetValue("IS_DAEMON", false);

    public static bool IsMultithreading(this IConfiguration config) =>
        config.GetValue("ENABLE_MULTITHREADING", false);

    public static bool IsDebugging(this IConfiguration config) =>
        config.GetValue("DEBUGGING", false);

    public static string GetGaugeProjectRoot(this IConfiguration config) =>
        config.GetValue<string>("GAUGE_PROJECT_ROOT");

    public static string GetGaugeCustomBuildPath(this IConfiguration config) =>
        config.GetValue<string>("GAUGE_CUSTOM_BUILD_PATH");

    public static string GetGaugeCSharpProjectFile(this IConfiguration config) =>
        config.GetValue<string>("GAUGE_CSHARP_PROJECT_FILE");

    public static string GetGaugeCSharpBuildArgs(this IConfiguration config) =>
        config.GetValue<string>("GAUGE_DOTNET_BUILD_ARGS");

    public static string GetGaugeCSharpRuntime(this IConfiguration config) =>
        config.GetValue<string>("GAUGE_DOTNET_RUNTIME");

    public static string GetGaugeCSharpConfig(this IConfiguration config) =>
        config.GetValue<string>("GAUGE_CSHARP_PROJECT_CONFIG") ?? "release";

    public static string GetGaugeLogLevel(this IConfiguration config) =>
        config.GetValue<string>("GAUGE_LOG_LEVEL");

    public static string GetGaugeClearStateFlag(this IConfiguration config) =>
        config.GetValue<string>("GAUGE_CLEAR_STATE_LEVEL");

    public static string GetGaugeBinDir(this IConfiguration config)
    {
        var customBuildPath = config.GetValue<string>("GAUGE_CUSTOM_BUILD_PATH");
        if (string.IsNullOrEmpty(customBuildPath))
            return Path.Combine(config.GetGaugeProjectRoot(), "gauge_bin");
        try
        {
            return Uri.TryCreate(customBuildPath, UriKind.Absolute, out _)
                ? customBuildPath
                : Path.Combine(config.GetGaugeProjectRoot(), customBuildPath);
        }
        catch (Exception)
        {
            return Path.Combine(config.GetGaugeProjectRoot(), "gauge_bin");
        }
    }

    public static int GetNumberOfParallelStreams(this IConfiguration config)
    {
        int numberOfStreams = 1;
        if (config.IsMultithreading())
        {
            var streamsCount = config.GetValue<string>("GAUGE_PARALLEL_STREAMS_COUNT");
            try
            {
                numberOfStreams = int.Parse(streamsCount);
                Logger.Debug($"Multithreading enabled, number of threads={numberOfStreams}");
            }
            catch (Exception e)
            {
                Logger.Debug($"Multithreading enabled, but could not read GAUGE_PARALLEL_STREAMS_COUNT as int. Got GAUGE_PARALLEL_STREAMS_COUNT={streamsCount}");
                Logger.Debug($"Using numberOfStreams=1, err: {e.Message}");
            }
        }
        return numberOfStreams;

    }
}
