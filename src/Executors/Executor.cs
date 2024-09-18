using Gauge.Dotnet.Processors;

namespace Gauge.Dotnet.Executors;

internal class Executor : IExecutor
{
    private readonly IServiceProvider _serviceProvider;

    public Executor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResult> Execute<TRequest, TResult>(int streamId, TRequest request)
    {
        var processor = _serviceProvider.GetRequiredService<IGaugeProcessor<TRequest, TResult>>();
        var result = processor.Process(streamId, request);
        return result;
    }
}
