/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


namespace Gauge.Dotnet.Models
{
    public class Diff
    {
        public Diff(string content, Range range)
        {
            Content = content;
            Range = range;
        }

        public string Content { get; internal set; }
        public Range Range { get; internal set; }
    }
}