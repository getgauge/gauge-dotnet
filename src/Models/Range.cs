/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


namespace Gauge.Dotnet.Models
{
    public class Range
    {
        public Range(Position start, Position end)
        {
            Start = start;
            End = end;
        }

        public Position Start { get; internal set; }
        public Position End { get; internal set; }
    }
}