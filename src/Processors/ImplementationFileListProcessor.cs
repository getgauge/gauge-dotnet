using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Helpers;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public class ImplementationFileListProcessor : IGaugeProcessor<Empty, ImplementationFileListResponse>
{
    private readonly IConfiguration _config;
    private readonly IAttributesLoader _attributesLoader;

    public ImplementationFileListProcessor(IConfiguration config, IAttributesLoader attributesLoader)
    {
        _config = config;
        _attributesLoader = attributesLoader;
    }

    public Task<ImplementationFileListResponse> Process(int stream, Empty request)
    {
        var response = new ImplementationFileListResponse();
        var classFiles = Directory.EnumerateFiles(_config.GetGaugeProjectRoot(), "*.cs", SearchOption.AllDirectories).ToList();

        var attributes = _attributesLoader.GetRemovedAttributes();
        foreach (var attribute in attributes)
        {
            classFiles.Remove(Path.Combine(_config.GetGaugeProjectRoot(), attribute.Value));
        }

        var removedFiles = FileHelper.GetRemovedDirFiles(_config);

        response.ImplementationFilePaths.AddRange(classFiles.Except(removedFiles));
        return Task.FromResult(response);
    }
}
