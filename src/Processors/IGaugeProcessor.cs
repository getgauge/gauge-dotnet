namespace Gauge.Dotnet.Processors;

public delegate IGaugeProcessor<TRequest, TResponse> GaugeProcessorDelegate<TRequest, TResponse>();

public interface IGaugeProcessor<TRequest, TResponse>
{
    Task<TResponse> Process(TRequest request);
}
