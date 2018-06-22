// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.Dotnet.Converters;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;
using NLog;

namespace Gauge.Dotnet
{
    public class StepExecutor : MethodExecutor, IStepExecutor
    {
        private readonly IAssemblyLoader _assemblyLoader;

        public StepExecutor(IAssemblyLoader assemblyLoader, IReflectionWrapper reflectionWrapper) : base(
            assemblyLoader.ClassInstanceManagerType,
            reflectionWrapper)
        {
            _assemblyLoader = assemblyLoader;
        }

        public ExecutionResult ExecuteStep(GaugeMethod gaugeMethod, string[] args)
        {
            {
                var method = gaugeMethod.MethodInfo;
                var executionResult = new ExecutionResult {Success = true};
                var logger = LogManager.GetLogger("Sandbox");
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
                    logger.Debug("Executing method: {0}", gaugeMethod.Name);
                    Execute(method, StringParamConverter.TryConvertParams(method, parameters));
                }
                catch (Exception ex)
                {
                    logger.Debug("Error executing {0}", method.Name);
                    var innerException = ex.InnerException ?? ex;
                    executionResult.ExceptionMessage = innerException.Message;
                    executionResult.StackTrace = innerException is AggregateException
                        ? innerException.ToString()
                        : innerException.StackTrace;
                    executionResult.Source = innerException.Source;
                    executionResult.Success = false;
                    executionResult.Recoverable = gaugeMethod.ContinueOnFailure;
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
}