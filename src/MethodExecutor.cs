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

using System;
using System.Reflection;
using Gauge.Dotnet.Wrappers;
using NLog;

namespace Gauge.Dotnet
{
    public class MethodExecutor
    {
        private object _classInstanceManager;
        private readonly Type _instanceManagerType;
        private readonly IReflectionWrapper _reflectionWrapper;

       
        protected MethodExecutor(Type type,
            IReflectionWrapper reflectionWrapper)
        {
            _instanceManagerType = type;
            _reflectionWrapper = reflectionWrapper;
        }

        public void SetClassInstanceManager(object classInstanceManager)
        {
            _classInstanceManager = classInstanceManager;
        }

        protected void Execute(MethodInfo method, params object[] parameters)
        {
            var typeToLoad = method.DeclaringType;
            var instance =
                _reflectionWrapper.InvokeMethod(_instanceManagerType, _classInstanceManager, "Get", typeToLoad);
            var logger = LogManager.GetLogger("ExecutionHelper");
            if (instance == null)
            {
                var error = "Could not load instance type: " + typeToLoad;
                logger.Error(error);
                throw new Exception(error);
            }

            _reflectionWrapper.Invoke(method, instance, parameters);
        }
    }
}