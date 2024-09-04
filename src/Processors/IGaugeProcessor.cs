namespace Gauge.Dotnet.Processors;

public interface IGaugeProcessor<TRequest, TResponse>
{
    Task<TResponse> Process(int stream, TRequest request);
}
