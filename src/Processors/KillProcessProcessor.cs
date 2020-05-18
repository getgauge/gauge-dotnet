/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class KillProcessProcessor : IMessageProcessor
    {
        public Message Process(Message request)
        {
            return request;
        }
    }
}