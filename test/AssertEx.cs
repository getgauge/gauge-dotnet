/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.UnitTests
{
    public static class AssertEx
    {
        public static void InheritsFrom<TBase, TDerived>()
        {
            ClassicAssert.True(typeof(TDerived).IsSubclassOf(typeof(TBase)),
                string.Format("Expected {0} to be a subclass of {1}", typeof(TDerived).FullName,
                    typeof(TBase).FullName));
        }

        public static void DoesNotInheritsFrom<TBase, TDerived>()
        {
            ClassicAssert.False(typeof(TDerived).IsSubclassOf(typeof(TBase)),
                string.Format("Expected {0} to NOT be a subclass of {1}", typeof(TDerived).FullName,
                    typeof(TBase).FullName));
        }

        public static void ContainsMethods(IEnumerable<MethodInfo> methodInfos, params string[] methodNames)
        {
            var existingMethodNames = methodInfos.Select(info => info.Name).ToArray();
            foreach (var methodName in methodNames)
                ClassicAssert.Contains(methodName, existingMethodNames);
        }

        public static IEnumerable<string> ExecuteProtectedMethod<T>(string methodName, params object[] methodParams)
        {
            var uninitializedObject = RuntimeHelpers.GetUninitializedObject(typeof(T));
            var tags = (IEnumerable<string>)uninitializedObject.GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(uninitializedObject, methodParams);
            return tags;
        }
    }
}