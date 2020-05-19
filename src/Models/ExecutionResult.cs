/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;

namespace Gauge.Dotnet.Models
{
    public class ExecutionResult : MarshalByRefObject
    {
        public bool Success { get; set; }

        public string ExceptionMessage { get; set; }

        public string Source { get; set; }

        public string StackTrace { get; set; }

        public byte[] ScreenShot { get; set; }

        public bool Recoverable { get; set; }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}