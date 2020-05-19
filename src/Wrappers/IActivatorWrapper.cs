/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;

namespace Gauge.Dotnet.Wrappers
{
    public interface IActivatorWrapper
    {
        object CreateInstance(Type t, params object[] args);
    }
}