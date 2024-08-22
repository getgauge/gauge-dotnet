using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Gauge.Dotnet;

public sealed class GaugeLoggingFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable _optionsReloadToken;
    private ConsoleFormatterOptions _formatterOptions;

    public GaugeLoggingFormatter(IOptionsMonitor<ConsoleFormatterOptions> options)
        : base(nameof(GaugeLoggingFormatter))
    {
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        _formatterOptions = options.CurrentValue;
    }

    private void ReloadLoggerOptions(ConsoleFormatterOptions options) => _formatterOptions = options;

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
    {
        string message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        if (message is null)
        {
            return;
        }

        var entry = new GaugeLogEntry(GetGaugeLogLevel(logEntry.LogLevel), message);
        var entryString = JsonSerializer.Serialize(entry);

        textWriter.WriteLine(entryString);
    }

    private string GetGaugeLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => "debug",
            LogLevel.Trace => "debug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warning",
            LogLevel.Error => "error",
            LogLevel.Critical => "fatal",
            _ => "info"
        };
    }

    public void Dispose() => _optionsReloadToken?.Dispose();

    private record GaugeLogEntry(string logLevel, string message);
}
