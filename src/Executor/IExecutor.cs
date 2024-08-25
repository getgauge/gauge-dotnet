namespace Gauge.Dotnet.Executor;

public interface IExecutor
{
    Task<TResult> Execute<TRequest, TResult>(TRequest request);
}
