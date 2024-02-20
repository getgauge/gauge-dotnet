/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Gauge.CSharp.Core
{
    public class TcpClientWrapper : ITcpClientWrapper
    {
        private readonly TcpClient _tcpClient = new TcpClient();

        public TcpClientWrapper(int port)
        {
            try
            {
                _tcpClient.Connect(new IPEndPoint(IPAddress.Loopback, port));
            }
            catch (Exception e)
            {
                throw new Exception("Could not connect", e);
            }
        }

        public bool Connected => _tcpClient.Connected;

        public Stream GetStream()
        {
            return _tcpClient.GetStream();
        }

        public void Close()
        {
            _tcpClient.Close();
        }
    }
}