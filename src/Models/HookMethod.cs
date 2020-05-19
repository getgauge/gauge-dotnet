/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


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