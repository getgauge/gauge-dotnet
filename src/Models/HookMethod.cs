// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-Dotnet.
//
// Gauge-Dotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-Dotnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-Dotnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gauge.Dotnet.Extensions;

namespace Gauge.Dotnet.Models
{
    [Serializable]
    public class HookMethod : IHookMethod
    {
        public HookMethod(LibType hookType, MethodInfo methodInfo, IAssemblyLoader assemblyLoader)
        {
            Method = methodInfo.FullyQuallifiedName();
            FilterTags = Enumerable.Empty<string>();

            var type = assemblyLoader.GetLibType(hookType);
            if (!type.IsSubclassOf(assemblyLoader.GetLibType(LibType.FilteredHookAttribute)))
                return;
            var customAttributes = methodInfo.GetCustomAttributes(false);
            var filteredHookAttribute = customAttributes.FirstOrDefault(type.IsInstanceOfType);
            if (filteredHookAttribute == null) return;

            FilterTags = (string[]) GetPropValue(filteredHookAttribute, "FilterTags");

            var targetTagBehaviourType = assemblyLoader.GetLibType(LibType.TagAggregationBehaviourAttribute);
            dynamic tagAggregationBehaviourAttribute =
                customAttributes.FirstOrDefault(targetTagBehaviourType.IsInstanceOfType);

            if (tagAggregationBehaviourAttribute != null)
                TagAggregation = (int) GetPropValue(tagAggregationBehaviourAttribute, "TagAggregation");
        }

        public int TagAggregation { get; }

        public IEnumerable<string> FilterTags { get; }

        public string Method { get; }

        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
    }
}