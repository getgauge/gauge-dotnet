// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using Gauge.CSharp.Runner.Processors;
using Gauge.Messages;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests.Processors
{
    public class DefaultProcessorTests
    {
        [Fact]
        public void ShouldProcessMessage()
        {
            var request = new Message
            {
                MessageId = 20,
                MessageType = Message.Types.MessageType.ExecuteStep
            };

            var response = new DefaultProcessor().Process(request);
            var executionStatusResponse = response.ExecutionStatusResponse;

            Assert.Equal(20, response.MessageId);
            Assert.Equal(Message.Types.MessageType.ExecutionStatusResponse, response.MessageType);
            Assert.Equal(0, executionStatusResponse.ExecutionResult.ExecutionTime);
        }
    }
}