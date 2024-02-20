/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
namespace Gauge.CSharp.Core
{
    public class GaugeConnection : AbstractGaugeConnection
    {
        public GaugeConnection(ITcpClientWrapper tcpClientWrapper) : base(tcpClientWrapper)
        {
        }
    }
}