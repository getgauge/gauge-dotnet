/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.IO;

namespace Gauge.Dotnet.Wrappers
{
    public interface IDirectoryWrapper
    {
        IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption searchOption);
        bool Exists(string path);
    }
}