/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.Dotnet.Loaders;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public abstract class DataStoreInitProcessorBase
{
    private readonly IAssemblyLoader _assemblyLoader;
    private readonly DataStoreType _dataStoreType;

    protected DataStoreInitProcessorBase(DataStoreType type, IAssemblyLoader assemblyLoader)
    {
        _dataStoreType = type;
        _assemblyLoader = assemblyLoader;
    }

    protected ExecutionStatusResponse Process(int stream)
    {
        try
        {
            var factoryType = _assemblyLoader.GetLibType(LibType.DataStoreFactory);
            var methodInfo = factoryType.GetMethod("AddDataStore", BindingFlags.NonPublic | BindingFlags.Static);
            methodInfo.Invoke(null, new object[] { stream, _dataStoreType });
            return new ExecutionStatusResponse
            {
                ExecutionResult = new ProtoExecutionResult
                {
                    Failed = false,
                    ExecutionTime = 0
                }
            };
        }
        catch (Exception ex)
        {
            var executionResult = new ProtoExecutionResult
            {
                Failed = true,
                ExecutionTime = 0
            };
            var innerException = ex.InnerException ?? ex;
            executionResult.ErrorMessage = innerException.Message;
            executionResult.StackTrace = innerException is AggregateException
                ? innerException.ToString()
                : innerException.StackTrace;

            return new ExecutionStatusResponse { ExecutionResult = executionResult };
        }
    }
}