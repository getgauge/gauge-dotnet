/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;

namespace Gauge.Dotnet.Wrappers
{
    public interface IAssemblyWrapper
    {
        Assembly LoadFrom(string location);
        Assembly[] GetCurrentDomainAssemblies();
    }
}