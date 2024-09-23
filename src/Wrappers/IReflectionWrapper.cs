/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;

namespace Gauge.Dotnet.Wrappers;

public interface IReflectionWrapper
{
    MethodInfo GetMethod(Type type, string methodName);
    MethodInfo GetMethod(Type type, string methodName, BindingFlags bindAttrs);
    MethodInfo[] GetMethods(Type type);
    object Invoke(MethodInfo method, object obj, params object[] args);

    object InvokeMethod(Type type, object instance, string methodName, BindingFlags bindAttrs,
        params object[] args);

    object InvokeMethod(Type type, object instance, string methodName, params object[] args);
}