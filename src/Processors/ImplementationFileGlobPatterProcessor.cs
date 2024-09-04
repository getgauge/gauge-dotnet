using Gauge.Dotnet.Extensions;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class ImplementationFileGlobPatterProcessor : IGaugeProcessor<Empty, ImplementationFileGlobPatternResponse>
{
    private readonly IConfiguration _config;

    public ImplementationFileGlobPatterProcessor(IConfiguration config)
    {
        _config = config;
    }

    public Task<ImplementationFileGlobPatternResponse> Process(int stream, Empty request)
    {
        var response = new ImplementationFileGlobPatternResponse();
        response.GlobPatterns.Add($"{_config.GetGaugeProjectRoot()}/**/*.cs");
        return Task.FromResult(response);
    }
}
