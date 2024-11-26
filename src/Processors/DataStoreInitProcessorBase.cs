/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.DataStore;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors;

public abstract class DataStoreInitProcessorBase(DataStoreType type, IDataStoreFactory dataStoreFactory)
{
    protected ExecutionStatusResponse Process(int stream)
    {
        try
        {
            dataStoreFactory.AddDataStore(stream, type);
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