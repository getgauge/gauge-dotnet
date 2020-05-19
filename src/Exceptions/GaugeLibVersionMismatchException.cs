/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Runtime.Serialization;

namespace Gauge.Dotnet.Exceptions
{
    [Serializable]
    public class GaugeLibVersionMismatchException : Exception
    {
        private const string ExceptionMessageFormat =
            "The version of Gauge.CSharp.Lib in the project is inconpatible with the version of Gauge CSharp plugin.\n Expecting minimum version: {0}, found {1}";

        public GaugeLibVersionMismatchException(Version targetLibVersion, Version expectedLibVersion)
            : base(string.Format(ExceptionMessageFormat, expectedLibVersion, targetLibVersion))
        {
        }

        public GaugeLibVersionMismatchException()
        {
        }

        public GaugeLibVersionMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}