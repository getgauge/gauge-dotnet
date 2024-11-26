/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.CSharp.Lib;
using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.UnitTests.Helpers;
using Microsoft.Extensions.Logging;

namespace Gauge.Dotnet.UnitTests;

[TestFixture]
internal class StepExecutorTests
{
    private readonly Mock<IClassInstanceManager> _mockClassInstanceManager = new();
    private readonly Mock<IAssemblyLoader> _mockAssemblyLoader = new();
    private readonly Mock<IExecutionInfoMapper> _mockExecutionInfoMapper = new();
    private readonly Mock<ILogger<StepExecutor>> _logger = new();

    [Test]
    public async Task ShoudExecuteStep()
    {
        var methodInfo = new MockMethodBuilder(_mockAssemblyLoader)
            .WithName("StepImplementation")
            .WithDeclaringTypeName("my.foo.type")
            .Build();
        var gaugeMethod = new GaugeMethod
        {
            Name = "StepImplementation",
            MethodInfo = methodInfo
        };

        _mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(typeof(IClassInstanceManager));
        _mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(_mockClassInstanceManager.Object);

        var executor = new StepExecutor(_mockAssemblyLoader.Object, _logger.Object, _mockExecutionInfoMapper.Object);

        var result = await executor.Execute(gaugeMethod, 1);
        ClassicAssert.True(result.Success);
    }

    [Test]
    public async Task ShoudExecuteStepAndGetFailure()
    {
        var methodInfo = new MockMethodBuilder(_mockAssemblyLoader)
            .WithName("StepImplementation")
            .WithDeclaringTypeName("my.foo.type")
            .Build();

        var gaugeMethod = new GaugeMethod
        {
            Name = "StepImplementation",
            MethodInfo = methodInfo
        };
        _mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(typeof(IClassInstanceManager));
        _mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(_mockClassInstanceManager.Object);

        var executor = new StepExecutor(_mockAssemblyLoader.Object, _logger.Object, _mockExecutionInfoMapper.Object);
        _mockClassInstanceManager.Setup(x => x.InvokeMethod(methodInfo, It.IsAny<CSharp.Lib.ExecutionContext>(), It.IsAny<object[]>()))
            .Throws(new Exception("step execution failure"));

        var result = await executor.Execute(gaugeMethod, 1);
        ClassicAssert.False(result.Success);
        ClassicAssert.AreEqual(result.ExceptionMessage, "step execution failure");
    }

    [Test]
    public async Task ShoudExecuteStepAndGetRecoverableError()
    {
        var methodInfo = new MockMethodBuilder(_mockAssemblyLoader)
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
        _mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(typeof(IClassInstanceManager));
        _mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(_mockClassInstanceManager.Object);

        var executor = new StepExecutor(_mockAssemblyLoader.Object, _logger.Object, _mockExecutionInfoMapper.Object);
        _mockClassInstanceManager.Setup(x => x.InvokeMethod(methodInfo, It.IsAny<CSharp.Lib.ExecutionContext>(), It.IsAny<object[]>()))
            .Throws(new Exception("step execution failure"));

        var result = await executor.Execute(gaugeMethod, 1);
        ClassicAssert.False(result.Success);
        ClassicAssert.True(result.Recoverable);
        ClassicAssert.AreEqual(result.ExceptionMessage, "step execution failure");
    }
}