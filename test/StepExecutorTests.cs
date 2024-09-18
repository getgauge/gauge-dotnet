/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.CSharp.Lib;
using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.UnitTests.Helpers;
using Microsoft.Extensions.Logging;

namespace Gauge.Dotnet.UnitTests;

[TestFixture]
internal class StepExecutorTests
{
    private readonly Mock<ILogger<StepExecutor>> _logger = new();

    [Test]
    public async Task ShoudExecuteStep()
    {
        var mockClassInstanceManager = new Mock<IClassInstanceManager>();

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

        mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(typeof(IClassInstanceManager));
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManager.Object);

        var executor = new StepExecutor(mockAssemblyLoader.Object, _logger.Object);

        var result = await executor.Execute(gaugeMethod, 1);
        ClassicAssert.True(result.Success);
    }

    [Test]
    public async Task ShoudExecuteStepAndGetFailure()
    {
        var mockClassInstanceManager = new Mock<IClassInstanceManager>();

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
        mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(typeof(IClassInstanceManager));
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManager.Object);

        var executor = new StepExecutor(mockAssemblyLoader.Object, _logger.Object);
        mockClassInstanceManager.Setup(x => x.InvokeMethod(methodInfo, 1, It.IsAny<object[]>()))
            .Throws(new Exception("step execution failure"));

        var result = await executor.Execute(gaugeMethod, 1);
        ClassicAssert.False(result.Success);
        ClassicAssert.AreEqual(result.ExceptionMessage, "step execution failure");
    }

    [Test]
    public async Task ShoudExecuteStepAndGetRecoverableError()
    {
        var mockClassInstanceManager = new Mock<IClassInstanceManager>();

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
        mockAssemblyLoader.Setup(x => x.ClassInstanceManagerType).Returns(typeof(IClassInstanceManager));
        mockAssemblyLoader.Setup(x => x.GetClassInstanceManager()).Returns(mockClassInstanceManager.Object);

        var executor = new StepExecutor(mockAssemblyLoader.Object, _logger.Object);
        mockClassInstanceManager.Setup(x => x.InvokeMethod(methodInfo, 1, It.IsAny<object[]>()))
            .Throws(new Exception("step execution failure"));

        var result = await executor.Execute(gaugeMethod, 1);
        ClassicAssert.False(result.Success);
        ClassicAssert.True(result.Recoverable);
        ClassicAssert.AreEqual(result.ExceptionMessage, "step execution failure");
    }
}