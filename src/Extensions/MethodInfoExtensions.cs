// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

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