/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;

namespace Gauge.Dotnet.Exceptions
{
    public class NotAValidGaugeProjectException : Exception
    {
        private const string InvalidProjectMessage = "This is not a valid Gauge Project";

        public NotAValidGaugeProjectException()
            : base(InvalidProjectMessage)
        {
        }
    }
}