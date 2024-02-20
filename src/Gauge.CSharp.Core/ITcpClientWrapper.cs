/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/
using System.IO;

namespace Gauge.CSharp.Core
{
    public interface ITcpClientWrapper
    {
        bool Connected { get; }
        Stream GetStream();
        void Close();
    }
}