/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.Dotnet.Loaders;

namespace Gauge.Dotnet.Executors;

public abstract class MethodExecutor
{
    private readonly object _classInstanceManager;
    private readonly Type _classInstanceManagerType;

    protected ILogger Logger { get; }


    protected MethodExecutor(IAssemblyLoader assemblyLoader, ILogger logger)
    {
        _classInstanceManagerType = assemblyLoader.ClassInstanceManagerType;
        _classInstanceManager = assemblyLoader.GetClassInstanceManager();
        Logger = logger;
    }

    protected async Task Execute(MethodInfo method, object context, params object[] parameters)
    {
        var invokeMethod = _classInstanceManagerType.GetMethod("InvokeMethod");
        Logger.LogDebug("Calling InvokeMethod to call method {method}", method);
        var response = invokeMethod.Invoke(_classInstanceManager, [method, context, parameters]);
        if (response is Task task)
        {
            await task;
        }
    }
}