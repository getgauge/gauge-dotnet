/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.Dotnet.Registries;

namespace Gauge.Dotnet.Loaders;

public interface IAssemblyLoader
{
    Type ScreenshotWriter { get; }
    Type ClassInstanceManagerType { get; }
    IEnumerable<MethodInfo> GetMethods(LibType type);
    Type GetLibType(LibType type);
    IStepRegistry GetStepRegistry();
    object GetClassInstanceManager();
}