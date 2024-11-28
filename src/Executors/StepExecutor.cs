/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.Dotnet.Converters;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Models;

namespace Gauge.Dotnet.Executors;

public class StepExecutor : MethodExecutor, IStepExecutor
{
    private readonly IAssemblyLoader _assemblyLoader;
    private readonly IExecutionInfoMapper _executionInfoMapper;

    public StepExecutor(IAssemblyLoader assemblyLoader, ILogger<StepExecutor> logger, IExecutionInfoMapper mapper)
        : base(assemblyLoader, logger)
    {
        _assemblyLoader = assemblyLoader;
        _executionInfoMapper = mapper;
    }

    public async Task<ExecutionResult> Execute(GaugeMethod gaugeMethod, int streamId, params string[] args)
    {
        {
            var method = gaugeMethod.MethodInfo;
            var executionResult = new ExecutionResult
            {
                Success = true,
                SkipScenario = false
            };
            try
            {
                var parameters = args.Select(o =>
                {
                    try
                    {
                        return GetTable(o);
                    }
                    catch
                    {
                        return o;
                    }
                }).ToArray();
                var context = _executionInfoMapper.ExecutionContextFrom(null, streamId);
                await Execute(method, context, StringParamConverter.TryConvertParams(method, parameters));
                executionResult.Success = true;
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();
                if (baseException != null &&
                    baseException.GetType().Name.Contains("SkipScenario", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogDebug("Skipping scenario when executing method: {MethodName} : {ExceptionMessage}", method.Name, baseException.Message);
                    executionResult.ExceptionMessage = baseException.Message;
                    executionResult.StackTrace = baseException.StackTrace;
                    executionResult.Source = baseException.Source;
                    executionResult.Success = true;
                    executionResult.SkipScenario = true;
                }
                else
                {
                    Logger.LogDebug("Error executing {MethodName} : {ExceptionMessage}", method.Name, method.Name);
                    var innerException = ex.InnerException ?? ex;
                    executionResult.ExceptionMessage = innerException.Message;
                    executionResult.StackTrace = innerException is AggregateException
                        ? innerException.ToString()
                        : innerException.StackTrace;
                    executionResult.Source = innerException.Source;
                    executionResult.Recoverable = gaugeMethod.ContinueOnFailure;
                    executionResult.Success = false;
                }
            }

            return executionResult;
        }
    }

    private object GetTable(string jsonString)
    {
        var serializer = new DataContractJsonSerializer(_assemblyLoader.GetLibType(LibType.Table));
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
        {
            return serializer.ReadObject(ms);
        }
    }
}