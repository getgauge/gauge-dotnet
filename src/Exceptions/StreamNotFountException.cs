/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
 
using System;

namespace Gauge.Dotnet.Executor
{
    [Serializable]
    internal class StreamNotFountException : Exception
    {
        private static string _message = "Requested stream {0} not found.";

        public StreamNotFountException(int stream) : base(String.Format(_message, stream))
        {
        }
    }
}