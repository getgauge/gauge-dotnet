/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.Dotnet.Models;

namespace Gauge.Dotnet.Loaders;

public interface IAssemblyLoader
{
    List<Assembly> AssembliesReferencingGaugeLib { get; }
    Type ScreenshotWriter { get; }
    Type ClassInstanceManagerType { get; }
    IEnumerable<MethodInfo> GetMethods(LibType type);
    Type GetLibType(LibType type);
    IStepRegistry GetStepRegistry();
    object GetClassInstanceManager();
}