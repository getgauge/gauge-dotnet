/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.DataStore;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet.Executors;

public class ExecutionInfoMapper : IExecutionInfoMapper
{
    private readonly Type _executionContextType;
    private readonly IActivatorWrapper _activatorWrapper;
    private readonly ITableFormatter _tableFormatter;
    private readonly IDataStoreFactory _dataStoreFactory;

    public ExecutionInfoMapper(IAssemblyLoader assemblyLoader, IActivatorWrapper activatorWrapper, IDataStoreFactory dataStoreFactory, ITableFormatter tableFormatter)
    {
        _executionContextType = assemblyLoader.GetLibType(LibType.ExecutionContext);
        _activatorWrapper = activatorWrapper;
        _dataStoreFactory = dataStoreFactory;
        _tableFormatter = tableFormatter;
    }

    public dynamic ExecutionContextFrom(ExecutionInfo currentExecutionInfo, int streamId)
    {
        return _activatorWrapper.CreateInstance(_executionContextType, SpecificationFrom(currentExecutionInfo?.CurrentSpec),
            ScenarioFrom(currentExecutionInfo?.CurrentScenario),
            StepFrom(currentExecutionInfo?.CurrentStep),
            DataStoresFor(streamId));
    }

    private dynamic SpecificationFrom(SpecInfo currentSpec)
    {
        var executionContextSpecType = _executionContextType.GetNestedType("Specification");
        return currentSpec != null
            ? _activatorWrapper.CreateInstance(executionContextSpecType, currentSpec.Name, currentSpec.FileName, currentSpec.IsFailed,
                currentSpec.Tags.ToArray())
            : _activatorWrapper.CreateInstance(executionContextSpecType);
    }

    private dynamic ScenarioFrom(ScenarioInfo currentScenario)
    {
        var executionContextScenarioType = _executionContextType.GetNestedType("Scenario");
        if (currentScenario != null && currentScenario.Retries == null)
        {
            currentScenario.Retries = new ScenarioRetriesInfo { MaxRetries = 0, CurrentRetry = 0 };
        }
        return currentScenario != null
            ? _activatorWrapper.CreateInstance(executionContextScenarioType, currentScenario.Name, currentScenario.IsFailed,
                currentScenario.Tags.ToArray(), currentScenario.Retries.MaxRetries, currentScenario.Retries.CurrentRetry)
            : _activatorWrapper.CreateInstance(executionContextScenarioType);
    }

    private dynamic StepFrom(StepInfo currentStep)
    {
        var executionContextStepType = _executionContextType.GetNestedType("StepDetails"); ;
        if (currentStep == null || currentStep.Step == null)
            return _activatorWrapper.CreateInstance(executionContextStepType);

        var parameters = new List<List<string>>();
        foreach (var parameter in currentStep.Step.Parameters)
        {
            if (parameter.ParameterType == Parameter.Types.ParameterType.Static)
            {
                parameters.Add(new List<string> { "Static", parameter.Name, parameter.Value });
            }
            else if (parameter.ParameterType == Parameter.Types.ParameterType.Dynamic)
            {
                parameters.Add(new List<string> { "Dynamic", parameter.Name, parameter.Value });
            }
            else if (parameter.ParameterType == Parameter.Types.ParameterType.SpecialString)
            {
                parameters.Add(new List<string> { "Special", parameter.Name, parameter.Value });
            }
            else if (parameter.ParameterType == Parameter.Types.ParameterType.SpecialTable ||
                parameter.ParameterType == Parameter.Types.ParameterType.Table)
            {
                var asJSon = _tableFormatter.GetJSON(parameter.Table);
                parameters.Add(new List<string> { "Table", parameter.Name, asJSon });
            }
        }

        var inst = _activatorWrapper.CreateInstance(
            executionContextStepType,
            currentStep.Step.ActualStepText, currentStep.IsFailed,
            currentStep.StackTrace, currentStep.ErrorMessage,
            parameters);

        return inst;
    }

    private dynamic DataStoresFor(int streamId)
    {
        var streamDataStores = _dataStoreFactory.GetDataStoresByStream(streamId);
        var executionContextDataStoresType = _executionContextType.GetNestedType("CurrentDataStores");
        dynamic dataStores = _activatorWrapper.CreateInstance(executionContextDataStoresType);
        dataStores.SuiteDataStore = _dataStoreFactory.SuiteDataStore;
        dataStores.SpecDataStore = streamDataStores.GetValueOrDefault(DataStoreType.Spec);
        dataStores.ScenarioDataStore = streamDataStores.GetValueOrDefault(DataStoreType.Scenario);
        return dataStores;
    }

}