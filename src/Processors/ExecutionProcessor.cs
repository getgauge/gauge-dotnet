/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Messages;

namespace Gauge.Dotnet.Processors
{
    public class ExecutionProcessor
    {
        protected static Message WrapInMessage(ProtoExecutionResult executionResult, Message request)
        {
            var executionStatusResponse = new ExecutionStatusResponse
            {
                ExecutionResult = executionResult
            };
            return new Message
            {
                MessageId = request.MessageId,
                MessageType = Message.Types.MessageType.ExecutionStatusResponse,
                ExecutionStatusResponse = executionStatusResponse
            };
        }
    }
}