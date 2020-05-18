/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/



using System.Collections.Generic;

namespace Gauge.Dotnet.Models
{
    public class RefactoringChange
    {
        public string FileName { get; internal set; }
        public string FileContent { get; internal set; }
        public List<Diff> Diffs { get; set; }
    }
}