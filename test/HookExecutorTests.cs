/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.CSharp.Lib;
using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Strategy;
using Gauge.Dotnet.UnitTests.Helpers;
using Gauge.Messages;
using Microsoft.Extensions.Logging;
using static Gauge.Dotnet.Constants;
using ExecutionContext = Gauge.CSharp.Lib.ExecutionContext;

namespace Gauge.Dotnet.UnitTests;

[TestFixture]
internal class HookExecutorTests
{
    private readonly Mock<ILogger<HookExecutor>> mockLogger = new();

    [Test]
    public async Task ShoudExecuteHooks()
    {
        var mockClassInstanceManager = new Mock<IClassInstanceManager>();
        var mockHookRegistry = new Mock<IHookRegistry>();
        var mockLogger = new Mock<ILogger<HookExecutor>>();

        var mockAssemblyLoader = new Mock<IAssemblyLoader>();
        var type = LibType.BeforeSuite;
        var methodInfo = new MockMethodBuilder(mockAssemblyLoader)
            .WithName($"{type}Hook")
            .WithFilteredHook(type)
            .WithDeclaringTypeName("my.foo.type")
            .WithNoParameters()
            .Build();
        var method = new HookMethod(type, methodInfo, mockAssemblyLoader.Object);
        mockHookRegistry.Setup(x => x.BeforeSuiteHooks).Returns(new HashSet<IHookMethod> { method });
        mockHookRegistry.Setup(x => x.MethodFor($"my.foo.type.{type}Hook")).Returns(methodInfo);
        mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(typeof(IClassInstanceManager));
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManager.Object);

        var mockExecutionInfoMapper = new Mock<IExecutionInfoMapper>();
        mockExecutionInfoMapper.Setup(x => x.ExecutionContextFrom(It.IsAny<ExecutionInfo>())).Returns(new { });

        var executor = new HookExecutor(mockAssemblyLoader.Object, mockExecutionInfoMapper.Object, mockHookRegistry.Object, mockLogger.Object);

        var result = await executor.Execute("BeforeSuite", new HooksStrategy(), new List<string>(), 1, new ExecutionInfo());
        ClassicAssert.True(result.Success, $"Hook execution failed: {result.ExceptionMessage}\n{result.StackTrace}");
    }

    [Test]
    public async Task ShoudExecuteHooksWithExecutionContext()
    {
        var mockClassInstanceManager = new Mock<IClassInstanceManager>();
        var mockHookRegistry = new Mock<IHookRegistry>();

        var mockAssemblyLoader = new Mock<IAssemblyLoader>();
        var type = LibType.BeforeSuite;
        var methodInfo = new MockMethodBuilder(mockAssemblyLoader)
            .WithName($"{type}Hook")
            .WithFilteredHook(type)
            .WithDeclaringTypeName("my.foo.type")
            .WithParameters(new KeyValuePair<Type, string>(typeof(ExecutionContext), "context"))
            .Build();
        var method = new HookMethod(type, methodInfo, mockAssemblyLoader.Object);
        mockHookRegistry.Setup(x => x.BeforeSuiteHooks).Returns(new HashSet<IHookMethod> { method });
        mockHookRegistry.Setup(x => x.MethodFor($"my.foo.type.BeforeSuiteHook-ExecutionContextcontext")).Returns(methodInfo);
        mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(typeof(IClassInstanceManager));
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManager.Object);

        var executionInfo = new ExecutionInfo();
        var expectedExecutionInfo = new ExecutionContext();

        var mockExecutionInfoMapper = new Mock<IExecutionInfoMapper>();
        mockExecutionInfoMapper.Setup(x => x.ExecutionContextFrom(executionInfo)).Returns(expectedExecutionInfo);

        mockClassInstanceManager.Setup(x => x.InvokeMethod(methodInfo, 1, expectedExecutionInfo)).Verifiable();

        var executor = new HookExecutor(mockAssemblyLoader.Object, mockExecutionInfoMapper.Object, mockHookRegistry.Object, mockLogger.Object);

        var result = await executor.Execute("BeforeSuite", new HooksStrategy(), new List<string>(), 1, executionInfo);
        ClassicAssert.True(result.Success, $"Hook execution failed: {result.ExceptionMessage}\n{result.StackTrace}");
        mockClassInstanceManager.VerifyAll();
    }

    [Test]
    public async Task ShoudExecuteHooksAndGetTheError()
    {
        var mockClassInstanceManagerType = new Mock<IClassInstanceManager>();
        var mockHookRegistry = new Mock<IHookRegistry>();

        var mockAssemblyLoader = new Mock<IAssemblyLoader>();
        var type = LibType.BeforeSuite;
        var methodInfo = new MockMethodBuilder(mockAssemblyLoader)
            .WithName($"{type}Hook")
            .WithFilteredHook(type)
            .WithDeclaringTypeName("my.foo.type")
            .Build();
        var method = new HookMethod(type, methodInfo, mockAssemblyLoader.Object);
        mockHookRegistry.Setup(x => x.BeforeSuiteHooks).Returns(new HashSet<IHookMethod> { method });
        mockHookRegistry.Setup(x => x.MethodFor($"my.foo.type.BeforeSuiteHook")).Returns(methodInfo);
        mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(typeof(IClassInstanceManager));
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManagerType.Object);

        var expectedExecutionInfo = new ExecutionContext();

        var mockExecutionInfoMapper = new Mock<IExecutionInfoMapper>();
        mockExecutionInfoMapper.Setup(x => x.ExecutionContextFrom(It.IsAny<ExecutionInfo>()))
            .Returns(expectedExecutionInfo);
        var executor = new HookExecutor(mockAssemblyLoader.Object, mockExecutionInfoMapper.Object, mockHookRegistry.Object, mockLogger.Object);
        mockClassInstanceManagerType.Setup(x => x.InvokeMethod(methodInfo, 1, It.IsAny<object[]>()))
            .Throws(new Exception("hook failed"));

        var result = await executor.Execute("BeforeSuite", new HooksStrategy(), new List<string>(), 1, new ExecutionInfo());
        ClassicAssert.False(result.Success, "Hook execution passed, expected failure");
        ClassicAssert.AreEqual(result.ExceptionMessage, "hook failed");
    }
}