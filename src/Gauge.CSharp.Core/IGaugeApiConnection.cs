/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
using System.Collections.Generic;
using Gauge.Messages;
using Google.Protobuf;

namespace Gauge.CSharp.Core
{
    public interface IGaugeApiConnection
    {
        bool Connected { get; }
        IEnumerable<string> GetStepValues(IEnumerable<string> stepTexts, bool hasInlineTable);
        APIMessage WriteAndReadApiMessage(IMessage stepValueRequestMessage);
        void WriteMessage(IMessage request);
        IEnumerable<byte> ReadBytes();
        void Dispose();
    }
}