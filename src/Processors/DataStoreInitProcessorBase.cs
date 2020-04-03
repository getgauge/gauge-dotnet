// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-Dotnet.
//
// Gauge-Dotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-Dotnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-Dotnet.  If not, see <http://www.gnu.org/licenses/>.

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