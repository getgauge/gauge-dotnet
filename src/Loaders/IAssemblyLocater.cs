﻿/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/



namespace Gauge.Dotnet.Loaders;

public interface IAssemblyLocater
{
    IEnumerable<string> GetAssembliesReferencingGaugeLib();
    string GetTestAssembly();
}