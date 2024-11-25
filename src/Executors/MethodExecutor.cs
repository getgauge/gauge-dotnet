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


    protected MethodExecutor(IAssemblyLoader assemblyLoader)
    {
        _classInstanceManagerType = assemblyLoader.ClassInstanceManagerType;
        _classInstanceManager = assemblyLoader.GetClassInstanceManager();
    }

    protected async Task Execute(MethodInfo method, int streamId, params object[] parameters)
    {
        var invokeMethod = _classInstanceManagerType.GetMethod("InvokeMethod");
        var response = invokeMethod.Invoke(_classInstanceManager, new object[] { method, streamId, parameters });
        if (response is Task task)
        {
            await task;
        }
    }
}