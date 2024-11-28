/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Exceptions;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Wrappers;

namespace Gauge.Dotnet.Loaders;

public class AssemblyLocater : IAssemblyLocater
{
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly IConfiguration _config;

    public AssemblyLocater(IDirectoryWrapper directoryWrapper, IConfiguration config)
    {
        _directoryWrapper = directoryWrapper;
        _config = config;
    }

    public string GetTestAssembly()
    {
        var gaugeBinDir = _config.GetGaugeBinDir();
        try
        {
            return _directoryWrapper
                .EnumerateFiles(gaugeBinDir, "*.deps.json", SearchOption.TopDirectoryOnly)
                .First().Replace(".deps.json", ".dll");
        }
        catch (InvalidOperationException)
        {
            throw new GaugeTestAssemblyNotFoundException(gaugeBinDir);
        }
    }
}