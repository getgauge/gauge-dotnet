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


            var result = executor.ExecuteHooks("BeforeSuite", new HooksStrategy(), new List<string>(),
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

            var result = executor.ExecuteHooks("BeforeSuite", new HooksStrategy(), new List<string>(),
                new ExecutionContext());
            Assert.False(result.Success);
            Assert.AreEqual(result.ExceptionMessage, "hook failed");
        }
    }
}