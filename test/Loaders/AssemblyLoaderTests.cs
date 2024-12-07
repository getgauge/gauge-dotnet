/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.Dotnet.Exceptions;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Registries;
using Gauge.Dotnet.Wrappers;
using Microsoft.Extensions.Logging;
using static Gauge.Dotnet.Constants;

namespace Gauge.Dotnet.UnitTests.Loaders;

[TestFixture]
public class AssemblyLoaderTests
{
    [SetUp]
    public void Setup()
    {
        _mockAssembly = new Mock<Assembly>();
        _mockActivationWrapper = new Mock<IActivatorWrapper>();
        var mockStepAttributeType = new Mock<Type>();
        _mockStepMethod = new Mock<MethodInfo>();
        var mockStepAttribute = new Mock<Attribute>();
        _mockStepMethod.Setup(x => x.GetCustomAttributes(It.IsAny<bool>()))
            .Returns(new List<Attribute> { mockStepAttribute.Object }.ToArray());
        _mockStepMethod.Setup(x => x.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>()))
            .Returns(new List<Attribute> { mockStepAttribute.Object }.ToArray());
        mockStepAttributeType.Setup(x => x.IsInstanceOfType(mockStepAttribute.Object))
            .Returns(true);
        mockStepAttributeType.Setup(x => x.FullName).Returns(LibType.Step.FullName());
        var mockIClassInstanceManagerType = new Mock<Type>();
        mockIClassInstanceManagerType.Setup(x => x.FullName).Returns("Gauge.CSharp.Lib.IClassInstanceManager");
        _mockInstanceManagerType = new Mock<Type>();
        _mockInstanceManagerType.Setup(type => type.GetInterfaces())
            .Returns(new[] { mockIClassInstanceManagerType.Object });
        _mockInstanceManagerType.Setup(x => x.Name)
            .Returns("TestInstanceManager");

        var mockIScreenshotWriter = new Mock<Type>();
        mockIScreenshotWriter.Setup(x => x.FullName).Returns("Gauge.CSharp.Lib.ICustomScreenshotWriter");
        _mockScreenshotWriterType = new Mock<Type>();
        _mockScreenshotWriterType.Setup(x => x.Name)
            .Returns("TestScreenGrabber");
        _mockScreenshotWriterType.Setup(x => x.GetInterfaces())
            .Returns(new[] { mockIScreenshotWriter.Object });
        _assemblyName = new AssemblyName("Mock.Test.Assembly");
        _mockAssembly.Setup(assembly => assembly.ExportedTypes)
            .Returns(new[]
            {
                mockStepAttributeType.Object,
                _mockScreenshotWriterType.Object,
                _mockInstanceManagerType.Object
            });
        _mockAssembly.Setup(x => x.GetName()).Returns(_assemblyName);
        var libAssemblyName = new AssemblyName(GaugeLibAssemblyName)
        {
            Version = Assembly.GetExecutingAssembly().GetReferencedAssemblies().FirstOrDefault(x => x.Name == GaugeLibAssemblyName).Version
        };
        var mockGaugeScreenshotsType = new Mock<Type>();
        mockGaugeScreenshotsType.Setup(x => x.FullName).Returns("Gauge.CSharp.Lib.GaugeScreenshots");
        _mockAssembly.Setup(assembly => assembly.GetReferencedAssemblies())
            .Returns([libAssemblyName]);
        _mockLibAssembly = new Mock<Assembly>();
        _mockLibAssembly.Setup(x => x.GetName()).Returns(libAssemblyName);
        _mockLibAssembly.Setup(x => x.ExportedTypes)
            .Returns(new[] { mockStepAttributeType.Object, mockGaugeScreenshotsType.Object });
        _mockReflectionWrapper = new Mock<IReflectionWrapper>();
        _mockReflectionWrapper.Setup(r => r.GetMethods(mockStepAttributeType.Object))
            .Returns([_mockStepMethod.Object]);
        var mockScreenshotWriter = new Mock<object>();
        _mockActivationWrapper.Setup(x => x.CreateInstance(_mockScreenshotWriterType.Object)).Returns(mockScreenshotWriter);
        _mockReflectionWrapper.Setup(x => x.InvokeMethod(mockGaugeScreenshotsType.Object, null, "RegisterCustomScreenshotWriter",
            BindingFlags.Static | BindingFlags.Public, new[] { mockScreenshotWriter }));
        _mockGaugeLoadContext = new Mock<IGaugeLoadContext>();
        _mockGaugeLoadContext.Setup(x => x.LoadFromAssemblyName(It.Is<AssemblyName>(x => x.FullName == _assemblyName.FullName)))
            .Returns(_mockAssembly.Object);
        _mockGaugeLoadContext.Setup(x => x.LoadFromAssemblyName(It.Is<AssemblyName>(x => libAssemblyName.FullName.Contains(x.FullName))))
            .Returns(_mockLibAssembly.Object);
        _mockGaugeLoadContext.Setup(x => x.GetLoadedAssembliesReferencingGaugeLib())
            .Returns(new[] { _mockAssembly.Object });
        _mockLogger = new Mock<ILogger<AssemblyLoader>>();
        _assemblyLoader = new AssemblyLoader(_mockGaugeLoadContext.Object, _mockReflectionWrapper.Object, _mockActivationWrapper.Object,
            new StepRegistry(), _mockLogger.Object, (logger) => ["Mock.Test.Assembly.dll"]);
    }

    private AssemblyName _assemblyName;
    private Mock<Assembly> _mockAssembly;
    private Mock<Assembly> _mockLibAssembly;
    private AssemblyLoader _assemblyLoader;
    private Mock<IGaugeLoadContext> _mockGaugeLoadContext;
    private Mock<IReflectionWrapper> _mockReflectionWrapper;
    private Mock<IActivatorWrapper> _mockActivationWrapper;
    private Mock<ILogger<AssemblyLoader>> _mockLogger;
    private Mock<Type> _mockInstanceManagerType;
    private Mock<Type> _mockScreenshotWriterType;
    private Mock<MethodInfo> _mockStepMethod;
    private const string TmpLocation = "/tmp/location";

    [Test]
    public void ShouldGetClassInstanceManagerType()
    {
        ClassicAssert.AreEqual(_mockInstanceManagerType.Object.Name, _assemblyLoader.ClassInstanceManagerType.Name);
    }

    [Test]
    public void ShouldGetMethodsForGaugeAttribute()
    {
        ClassicAssert.Contains(_mockStepMethod.Object, _assemblyLoader.GetMethods(LibType.Step).ToList());
    }

    [Test]
    public void ShouldGetScreenGrabberType()
    {
        ClassicAssert.AreEqual(_mockScreenshotWriterType.Object.Name, _assemblyLoader.ScreenshotWriter.Name);
    }

    [Test]
    public void ShouldGetTargetAssembly()
    {
        _mockGaugeLoadContext.Verify(x => x.LoadFromAssemblyName(It.Is<AssemblyName>(a => a.FullName == _assemblyName.FullName)));
    }

    [Test]
    public void ShouldThrowExceptionWhenLibAssemblyNotFound()
    {
        var mockLogger = new Mock<ILogger<AssemblyLoader>>();
        var mockReflectionWrapper = new Mock<IReflectionWrapper>();
        var mockActivationWrapper = new Mock<IActivatorWrapper>();
        var mockGaugeLoadContext = new Mock<IGaugeLoadContext>();
        mockGaugeLoadContext.Setup(x => x.LoadFromAssemblyName(It.IsAny<AssemblyName>())).Throws<FileLoadException>();
        Assert.Throws<FileLoadException>(() => new AssemblyLoader(mockGaugeLoadContext.Object, mockReflectionWrapper.Object,
            mockActivationWrapper.Object, new StepRegistry(), mockLogger.Object, (logger) => []));
    }

    [Test]
    public void ShouldThrowExceptionWhenLibAssemblyDoesNotMatchVersionTooHigh()
    {
        var testVersion = Assembly.GetExecutingAssembly().GetReferencedAssemblies().FirstOrDefault(x => x.Name == GaugeLibAssemblyName).Version;
        var mockVersion = new Version(testVersion.Major, testVersion.Minor + 1, testVersion.Build);
        var libAssemblyName = new AssemblyName(GaugeLibAssemblyName)
        {
            Version = mockVersion
        };
        _mockLibAssembly.Setup(x => x.GetName()).Returns(libAssemblyName);

        var exception = Assert.Throws<GaugeLibVersionMismatchException>(() => new AssemblyLoader(_mockGaugeLoadContext.Object,
            _mockReflectionWrapper.Object, _mockActivationWrapper.Object, new StepRegistry(), _mockLogger.Object, (logger) => []));

        Assert.That(exception.Message, Contains.Substring($"Expecting minimum version: {testVersion.Major}.{testVersion.Minor}.0"));
        Assert.That(exception.Message, Contains.Substring($"and less than {testVersion.Major}.{testVersion.Minor + 1}.0"));
    }

    [Test]
    public void ShouldThrowExceptionWhenLibAssemblyDoesNotMatchVersionTooLow()
    {
        var testVersion = Assembly.GetExecutingAssembly().GetReferencedAssemblies().FirstOrDefault(x => x.Name == GaugeLibAssemblyName).Version;
        var mockVersion = new Version(testVersion.Major, testVersion.Minor - 1, testVersion.Build);
        var libAssemblyName = new AssemblyName(GaugeLibAssemblyName)
        {
            Version = mockVersion
        };
        _mockLibAssembly.Setup(x => x.GetName()).Returns(libAssemblyName);

        var exception = Assert.Throws<GaugeLibVersionMismatchException>(() => new AssemblyLoader(_mockGaugeLoadContext.Object,
            _mockReflectionWrapper.Object, _mockActivationWrapper.Object, new StepRegistry(), _mockLogger.Object, (logger) => []));

        Assert.That(exception.Message, Contains.Substring($"Expecting minimum version: {testVersion.Major}.{testVersion.Minor}.0"));
        Assert.That(exception.Message, Contains.Substring($"and less than {testVersion.Major}.{testVersion.Minor + 1}.0"));
    }
}