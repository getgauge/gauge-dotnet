/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using Gauge.Dotnet.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    internal class HookExecutorTests
    {
        [Test]
        public void ShoudExecuteHooks()
        {
            var mockInstance = new Mock<object>().Object;
            var mockClassInstanceManagerType = new Mock<Type>().Object;
            var mockClassInstanceManager = new Mock<object>().Object;

            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var type = LibType.BeforeSuite;
            var methodInfo = new MockMethodBuilder(mockAssemblyLoader)
                .WithName($"{type}Hook")
                .WithFilteredHook(type)
                .WithDeclaringTypeName("my.foo.type")
                .Build();
            mockAssemblyLoader.Setup(x => x.GetMethods(type)).Returns(new List<MethodInfo> {methodInfo});
            mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(mockClassInstanceManagerType);

            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            mockReflectionWrapper
                .Setup(x => x.InvokeMethod(mockClassInstanceManagerType, mockClassInstanceManager, "Get",
                    methodInfo.DeclaringType))
                .Returns(mockInstance);

            var executor = new HookExecutor(mockAssemblyLoader.Object, mockReflectionWrapper.Object,
                mockClassInstanceManager);
            mockReflectionWrapper.Setup(x => x.Invoke(methodInfo, mockInstance, new List<object>()))
                .Returns(null);


            var result = executor.Execute("BeforeSuite", new HooksStrategy(), new List<string>(),
                new ExecutionContext());
            Assert.True(result.Success);
        }

        [Test]
        public void ShoudExecuteHooksAndGetTheError()
        {
            var mockInstance = new Mock<object>().Object;
            var mockClassInstanceManagerType = new Mock<Type>().Object;
            var mockClassInstanceManager = new Mock<object>().Object;

            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var type = LibType.BeforeSuite;
            var methodInfo = new MockMethodBuilder(mockAssemblyLoader)
                .WithName($"{type}Hook")
                .WithFilteredHook(type)
                .WithDeclaringTypeName("my.foo.type")
                .Build();
            mockAssemblyLoader.Setup(x => x.GetMethods(type)).Returns(new List<MethodInfo> {methodInfo});
            mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(mockClassInstanceManagerType);

            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            mockReflectionWrapper
                .Setup(x => x.InvokeMethod(mockClassInstanceManagerType, mockClassInstanceManager, "Get",
                    methodInfo.DeclaringType))
                .Returns(mockInstance);

            var executor = new HookExecutor(mockAssemblyLoader.Object, mockReflectionWrapper.Object,
                mockClassInstanceManager);
            mockReflectionWrapper.Setup(x => x.Invoke(methodInfo, mockInstance))
                .Throws(new Exception("hook failed"));

            var result = executor.Execute("BeforeSuite", new HooksStrategy(), new List<string>(),
                new ExecutionContext());
            Assert.False(result.Success);
            Assert.AreEqual(result.ExceptionMessage, "hook failed");
        }
    }
}