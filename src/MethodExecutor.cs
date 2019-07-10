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
using System.Reflection;
using Gauge.Dotnet.Wrappers;

namespace Gauge.Dotnet
{
    public class MethodExecutor
    {
        private readonly object _classInstanceManager;
        private readonly Type _classInstanceManagerType;
        private readonly IReflectionWrapper _reflectionWrapper;


        protected MethodExecutor(Type type,
            IReflectionWrapper reflectionWrapper, object classInstanceManager)
        {
            _classInstanceManagerType = type;
            _reflectionWrapper = reflectionWrapper;
            _classInstanceManager = classInstanceManager;
        }

        protected void Execute(MethodInfo method, params object[] parameters)
        {
            var typeToLoad = method.DeclaringType;
            var instance =
                _reflectionWrapper.InvokeMethod(_classInstanceManagerType, _classInstanceManager, "Get", typeToLoad);
            if (instance == null)
            {
                var error = "Could not load instance type: " + typeToLoad;
                Logger.Error(error);
                throw new Exception(error);
            }

            _reflectionWrapper.Invoke(method, instance, parameters);
        }
    }
}