using Gauge.Dotnet.Processors;

namespace Gauge.Dotnet.Executor;

internal class Executor : IExecutor
{
    private readonly IServiceProvider _serviceProvider;

    public Executor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResult> Execute<TRequest, TResult>(TRequest request)
    {
        var processor = _serviceProvider.GetRequiredService<IGaugeProcessor<TRequest, TResult>>();
        return processor.Process(request);
    }
}
