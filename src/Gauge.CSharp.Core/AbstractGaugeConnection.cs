/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace Gauge.CSharp.Core
{
    public abstract class AbstractGaugeConnection : IDisposable
    {
        protected readonly ITcpClientWrapper TcpClientWrapper;

        protected AbstractGaugeConnection(ITcpClientWrapper tcpClientWrapper)
        {
            TcpClientWrapper = tcpClientWrapper;
        }

        public bool Connected => TcpClientWrapper.Connected;

        public void Dispose()
        {
            TcpClientWrapper.Close();
        }

        public void WriteMessage(IMessage request)
        {
            var bytes = request.ToByteArray();
            var cos = new CodedOutputStream(TcpClientWrapper.GetStream());
            cos.WriteUInt64((ulong) bytes.Length);
            cos.Flush();
            TcpClientWrapper.GetStream().Write(bytes, 0, bytes.Length);
            TcpClientWrapper.GetStream().Flush();
        }

        public IEnumerable<byte> ReadBytes()
        {
            var networkStream = TcpClientWrapper.GetStream();
            var codedInputStream = new CodedInputStream(networkStream);
            return codedInputStream.ReadBytes();
        }

        protected static long GenerateMessageId()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}