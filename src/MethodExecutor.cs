/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        protected Task Execute(MethodInfo method, params object[] parameters)
        {
            var typeToLoad = method.DeclaringType;
            var instance =
                _reflectionWrapper.InvokeMethod(_classInstanceManagerType, _classInstanceManager, "Get", typeToLoad);
            if (instance == null)
            {
                var error = "Could not load instance type: " + typeToLoad;
                Logger.Error(error);
                throw new TypeLoadException(error);
            }

            if (method.GetCustomAttributes(typeof(AsyncStateMachineAttribute), false).Any())
            {
                return (Task)_reflectionWrapper.Invoke(method, instance, parameters);
            }

            return Task.Run(() => _reflectionWrapper.Invoke(method, instance, parameters));
        }
    }
}