/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using Gauge.Dotnet.Processors;
using Gauge.Messages;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.UnitTests.Processors
{
    [TestFixture]
    public class DefaultProcessorTests
    {
        [Test]
        public void ShouldProcessMessage()
        {
            var request = new Message
            {
                MessageId = 20,
                MessageType = Message.Types.MessageType.ExecuteStep
            };

            var response = new DefaultProcessor().Process(request);
            var executionStatusResponse = response.ExecutionStatusResponse;

            ClassicAssert.AreEqual(response.MessageId, 20);
            ClassicAssert.AreEqual(response.MessageType, Message.Types.MessageType.ExecutionStatusResponse);
            ClassicAssert.AreEqual(executionStatusResponse.ExecutionResult.ExecutionTime, 0);
        }
    }
}