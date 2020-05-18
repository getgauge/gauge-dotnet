/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Gauge.Dotnet.Models
{
    [Serializable]
    public class GaugeMethod
    {
        public MethodInfo MethodInfo { get; set; }
        public string Name { get; set; }
        public int ParameterCount { get; set; }
        public bool ContinueOnFailure { get; set; }
        public string StepText { get; set; }
        public string StepValue { get; set; }
        public string FileName { get; set; }
        public string ClassName { get; set; }
        public FileLinePositionSpan Span { get; internal set; }
        public bool HasAlias { get; set; }
        public IEnumerable<string> Aliases { get; set; }
         public bool IsExternal {get; set;}
    }
}