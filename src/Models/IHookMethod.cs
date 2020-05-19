/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;

namespace Gauge.Dotnet.Models
{
    public interface IHookMethod
    {
        string Method { get; }
        int TagAggregation { get; }
        IEnumerable<string> FilterTags { get; }
    }
}