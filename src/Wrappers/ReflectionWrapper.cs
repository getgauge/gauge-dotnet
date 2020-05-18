/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Reflection;

namespace Gauge.Dotnet.Wrappers
{
    public class ReflectionWrapper : IReflectionWrapper
    {
        public MethodInfo GetMethod(Type type, string methodName)
        {
            return type.GetMethod(methodName);
        }

        public MethodInfo[] GetMethods(Type type)
        {
            return type.GetMethods();
        }

        public object Invoke(MethodInfo method, object obj, params object[] args)
        {
            return method.Invoke(obj, args);
        }

        public object InvokeMethod(Type type, object instance, string methodName, params object[] args)
        {
            var method = GetMethod(type, methodName);
            return Invoke(method, instance, args);
        }

        public object InvokeMethod(Type type, object instance, string methodName, BindingFlags bindingAttrs,
            params object[] args)
        {
            var method = type.GetMethod(methodName, bindingAttrs);
            return Invoke(method, instance, args);
        }
    }
}