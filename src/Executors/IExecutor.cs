namespace Gauge.Dotnet.Executors;

public interface IExecutor
{
    Task<TResult> Execute<TRequest, TResult>(int stream, TRequest request);
}
