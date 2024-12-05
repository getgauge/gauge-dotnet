/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Microsoft.Extensions.FileProviders;

namespace Gauge.Dotnet.Exceptions;

public class GaugeTestAssemblyNotFoundException : FileNotFoundException
{
    private const string TestAssemblyNotFoundMessageFormat
        = "Could not locate the target test assembly. Gauge-Dotnet could not find a deps.json file in {0}";

    public GaugeTestAssemblyNotFoundException(IFileProvider fileProvider)
        : base(string.Format(TestAssemblyNotFoundMessageFormat, (fileProvider as PhysicalFileProvider)?.Root ?? string.Empty))
    {
    }
}