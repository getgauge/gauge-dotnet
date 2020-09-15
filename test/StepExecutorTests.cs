/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Threading;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.UnitTests.Helpers;
using Gauge.Dotnet.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    internal class StepExecutorTests
    {
        [Test]
        public void ShoudExecuteStep()
        {
            var mockInstance = new Mock<object>().Object;
            var mockClassInstanceManagerType = new Mock<Type>().Object;
            var mockClassInstanceManager = new ThreadLocal<object>(() => new Mock<object>().Object);

            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var methodInfo = new MockMethodBuilder(mockAssemblyLoader)
                .WithName("StepImplementation")
                .WithDeclaringTypeName("my.foo.type")
                .Build();
            var gaugeMethod = new GaugeMethod
            {
                Name = "StepImplementation",
                MethodInfo = methodInfo
            };

            mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(mockClassInstanceManagerType);

            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            mockReflectionWrapper
                .Setup(x => x.InvokeMethod(mockClassInstanceManagerType, mockClassInstanceManager, "Get",
                    methodInfo.DeclaringType))
                .Returns(mockInstance);

            var executor = new StepExecutor(mockAssemblyLoader.Object, mockReflectionWrapper.Object,
                mockClassInstanceManager);
            mockReflectionWrapper.Setup(x => x.Invoke(methodInfo, mockInstance))
                .Returns(null);


            var result = executor.Execute(gaugeMethod);
            Assert.True(result.Success);
        }

        [Test]
        public void ShoudExecuteStepAndGetFailure()
        {
            var mockInstance = new Mock<object>().Object;
            var mockClassInstanceManagerType = new Mock<Type>().Object;
            var mockClassInstanceManager = new ThreadLocal<object>(() => new Mock<object>().Object);

            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var methodInfo = new MockMethodBuilder(mockAssemblyLoader)
                .WithName("StepImplementation")
                .WithDeclaringTypeName("my.foo.type")
                .Build();

            var gaugeMethod = new GaugeMethod
            {
                Name = "StepImplementation",
                MethodInfo = methodInfo
            };
            mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(mockClassInstanceManagerType);

            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            mockReflectionWrapper
                .Setup(x => x.InvokeMethod(mockClassInstanceManagerType, mockClassInstanceManager, "Get",
                    methodInfo.DeclaringType))
                .Returns(mockInstance);

            var executor = new StepExecutor(mockAssemblyLoader.Object, mockReflectionWrapper.Object,
                mockClassInstanceManager);
            mockReflectionWrapper.Setup(x => x.Invoke(methodInfo, mockInstance))
                .Throws(new Exception("step execution failure"));

            var result = executor.Execute(gaugeMethod);
            Assert.False(result.Success);
            Assert.AreEqual(result.ExceptionMessage, "step execution failure");
        }

        [Test]
        public void ShoudExecuteStepAndGetRecoverableError()
        {
            var mockInstance = new Mock<object>().Object;
            var mockClassInstanceManagerType = new Mock<Type>().Object;
            var mockClassInstanceManager = new ThreadLocal<object>(() => new Mock<object>().Object);

            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var methodInfo = new MockMethodBuilder(mockAssemblyLoader)
                .WithName("StepImplementation")
                .WithContinueOnFailure()
                .WithDeclaringTypeName("my.foo.type")
                .Build();

            var gaugeMethod = new GaugeMethod
            {
                Name = "StepImplementation",
                MethodInfo = methodInfo,
                ContinueOnFailure = true
            };
            mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(mockClassInstanceManagerType);

            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            mockReflectionWrapper
                .Setup(x => x.InvokeMethod(mockClassInstanceManagerType, mockClassInstanceManager, "Get",
                    methodInfo.DeclaringType))
                .Returns(mockInstance);

            var executor = new StepExecutor(mockAssemblyLoader.Object, mockReflectionWrapper.Object,
                mockClassInstanceManager);
            mockReflectionWrapper.Setup(x => x.Invoke(methodInfo, mockInstance))
                .Throws(new Exception("step execution failure"));

            var result = executor.Execute(gaugeMethod);
            Assert.False(result.Success);
            Assert.True(result.Recoverable);
            Assert.AreEqual(result.ExceptionMessage, "step execution failure");
        }
    }
}