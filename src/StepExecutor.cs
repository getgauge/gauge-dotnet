/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Gauge.Dotnet.Converters;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;
using Gauge.CSharp.Lib.Attribute;

namespace Gauge.Dotnet
{
    public class StepExecutor : MethodExecutor, IStepExecutor
    {
        private readonly IAssemblyLoader _assemblyLoader;

        public StepExecutor(IAssemblyLoader assemblyLoader, IReflectionWrapper reflectionWrapper,
            object classInstanceMananger) : base(
            assemblyLoader.ClassInstanceManagerType,
            reflectionWrapper, classInstanceMananger)
        {
            _assemblyLoader = assemblyLoader;
        }

        public ExecutionResult Execute(GaugeMethod gaugeMethod, params string[] args)
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
                    Logger.Debug($"Executing method: {gaugeMethod.Name}");
                    Execute(method, StringParamConverter.TryConvertParams(method, parameters));
                    executionResult.Success = true;
                }
                catch (Exception ex)
                {
                    var baseException = ex.GetBaseException();
                    if (baseException != null && 
                        baseException.GetType().Name.Contains("SkipScenario", StringComparison.OrdinalIgnoreCase)) {
                        Logger.Debug($"Skipping scenario when executing method: {method.Name} : {baseException.Message}") ;
                        executionResult.ExceptionMessage = baseException.Message;
                        executionResult.StackTrace = baseException.StackTrace;
                        executionResult.Source = baseException.Source;
                        executionResult.Success = true;
                        executionResult.SkipScenario = true;
                    }
                    else {
                        Logger.Debug($"Error executing {method.Name} : {ex.Message}") ;
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
}