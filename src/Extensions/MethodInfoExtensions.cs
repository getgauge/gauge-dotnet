/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Linq;
using System.Reflection;

namespace Gauge.Dotnet.Extensions
{
    public static class MethodInfoExtensions
    {
        public static string FullyQuallifiedName(this MethodInfo info)
        {
            var parameters = info.GetParameters();
            var parameterText = parameters.Length > 0
                ? "-" + parameters.Select(parameterInfo =>
                          string.Concat(parameterInfo.ParameterType.Name, parameterInfo.Name))
                      .Aggregate(string.Concat)
                : string.Empty;

            return info.DeclaringType == null
                ? info.Name
                : string.Format("{0}.{1}{2}", info.DeclaringType.FullName, info.Name, parameterText);
        }

        public static bool IsRecoverableStep(this MethodInfo info, IAssemblyLoader assemblyLoader)
        {
            var stepType = assemblyLoader.GetLibType(LibType.Step);
            var continueOnFailureType = assemblyLoader.GetLibType(LibType.ContinueOnFailure);
            var customAttributes = info.GetCustomAttributes(false).ToList();
            return customAttributes.Any(stepType.IsInstanceOfType)
                   && customAttributes.Any(continueOnFailureType.IsInstanceOfType);
        }
    }
}