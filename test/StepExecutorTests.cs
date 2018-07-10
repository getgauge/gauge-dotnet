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
            var mockClassInstanceManager = new Mock<object>().Object;

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
            var mockClassInstanceManager = new Mock<object>().Object;

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
            var mockClassInstanceManager = new Mock<object>().Object;

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