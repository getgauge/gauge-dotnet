/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
using System.Collections.Generic;
using System.Linq;
using Gauge.Messages;
using Google.Protobuf;

namespace Gauge.CSharp.Core
{
    public class GaugeApiConnection : AbstractGaugeConnection, IGaugeApiConnection
    {
        public GaugeApiConnection(ITcpClientWrapper clientWrapper) : base(clientWrapper)
        {
        }

        public IEnumerable<string> GetStepValues(IEnumerable<string> stepTexts, bool hasInlineTable)
        {
            foreach (var stepText in stepTexts)
            {
                var stepValueRequest = new GetStepValueRequest
                {
                    StepText = stepText,
                    HasInlineTable = hasInlineTable
                };
                var stepValueRequestMessage = new APIMessage
                {
                    MessageId = GenerateMessageId(),
                    MessageType = APIMessage.Types.APIMessageType.GetStepValueRequest,
                    StepValueRequest = stepValueRequest
                };
                var apiMessage = WriteAndReadApiMessage(stepValueRequestMessage);
                yield return apiMessage.StepValueResponse.StepValue.StepValue;
            }
        }

        public APIMessage WriteAndReadApiMessage(IMessage stepValueRequestMessage)
        {
            lock (TcpClientWrapper)
            {
                WriteMessage(stepValueRequestMessage);
                return ReadMessage();
            }
        }

        private APIMessage ReadMessage()
        {
            var responseBytes = ReadBytes();
            return APIMessage.Parser.ParseFrom(responseBytes.ToArray());
        }
    }
}