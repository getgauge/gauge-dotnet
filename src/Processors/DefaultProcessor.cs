/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class DefaultProcessor : IMessageProcessor
    {
        public Message Process(Message request)
        {
            return GetResponseMessage(request);
        }

        private static Message GetResponseMessage(Message request)
        {
            var response = new ExecutionStatusResponse
            {
                ExecutionResult = new ProtoExecutionResult
                {
                    Failed = false,
                    ExecutionTime = 0
                }
            };
            return new Message
            {
                MessageId = request.MessageId,
                MessageType = Message.Types.MessageType.ExecutionStatusResponse,
                ExecutionStatusResponse = response
            };
        }
    }
}