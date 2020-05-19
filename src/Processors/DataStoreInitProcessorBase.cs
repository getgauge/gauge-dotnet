/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public abstract class DataStoreInitProcessorBase
    {
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly DataStoreType _dataStoreType;

        protected DataStoreInitProcessorBase(IAssemblyLoader assemblyLoader, DataStoreType type)
        {
            _assemblyLoader = assemblyLoader;
            _dataStoreType = type;
        }

        public ExecutionStatusResponse Process()
        {
            try
            {
                var initMethod = _assemblyLoader.GetLibType(LibType.DataStoreFactory)
                    .GetMethod($"Initialize{_dataStoreType}DataStore");
                initMethod.Invoke(null, null);
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
}