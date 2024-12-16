/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

namespace Gauge.Dotnet.Exceptions;

[Serializable]
public class GaugeLibVersionMismatchException : Exception
{
    public GaugeLibVersionMismatchException(Version expectedLibVersion)
        : base($"The version of {GaugeLibAssemblyName} in the project is incompatible with the version of Gauge CSharp plugin.\n" +
              $"Expecting minimum version: {expectedLibVersion.Major}.{expectedLibVersion.Minor}.0, " +
              $"and less than {expectedLibVersion.Major}.{expectedLibVersion.Minor + 1}.0")

    {
    }
}