/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System.Diagnostics.CodeAnalysis;

namespace Gauge.Dotnet.Wrappers;

[ExcludeFromCodeCoverage]
public class ActivatorWrapper(IServiceProvider serviceProvider) : IActivatorWrapper
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public object CreateInstance(Type t, params object[] args)
    {
        return ActivatorUtilities.CreateInstance(_serviceProvider, t, args);
    }
}