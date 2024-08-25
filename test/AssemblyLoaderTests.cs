/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Reflection;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;

namespace Gauge.Dotnet.UnitTests;

[TestFixture]
public class AssemblyLoaderTests
{
    [SetUp]
    public void Setup()
    {
        Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TmpLocation);
        _assemblyLocation = "/foo/location";
        _mockAssembly = new Mock<Assembly>();
        var mockActivationWrapper = new Mock<IActivatorWrapper>();
        var mockStepAttributeType = new Mock<Type>();
        _mockStepMethod = new Mock<MethodInfo>();
        var mockStepAttribute = new Mock<Attribute>();
        _mockStepMethod.Setup(x => x.GetCustomAttributes(It.IsAny<bool>()))
            .Returns((new List<Attribute> { mockStepAttribute.Object }).ToArray());
        _mockStepMethod.Setup(x => x.GetCustomAttributes(It.IsAny<Type>(), It.IsAny<bool>()))
            .Returns((new List<Attribute> { mockStepAttribute.Object }).ToArray());
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
        _mockAssembly.Setup(x => x.GetName())
            .Returns(_assemblyName);
        var libAssemblyName = new AssemblyName("Gauge.CSharp.Lib");
        var mockGaugeScreenshotsType = new Mock<Type>();
        mockGaugeScreenshotsType.Setup(x => x.FullName).Returns("Gauge.CSharp.Lib.GaugeScreenshots");
        _mockAssembly.Setup(assembly => assembly.GetReferencedAssemblies())
            .Returns(new[] { libAssemblyName });
        _mockLibAssembly = new Mock<Assembly>();
        _mockLibAssembly.Setup(x => x.GetName()).Returns(libAssemblyName);
        _mockLibAssembly.Setup(x => x.ExportedTypes)
            .Returns(new[] { mockStepAttributeType.Object, mockGaugeScreenshotsType.Object });
        var mockReflectionWrapper = new Mock<IReflectionWrapper>();
        mockReflectionWrapper.Setup(r => r.GetMethods(mockStepAttributeType.Object))
            .Returns(new[] { _mockStepMethod.Object });
        var mockScreenshotWriter = new Mock<object>();
        mockActivationWrapper.Setup(x => x.CreateInstance(_mockScreenshotWriterType.Object)).Returns(mockScreenshotWriter);
        mockReflectionWrapper.Setup(x => x.InvokeMethod(mockGaugeScreenshotsType.Object, null, "RegisterCustomScreenshotWriter",
            BindingFlags.Static | BindingFlags.Public, new[] { mockScreenshotWriter }));
        _mockGaugeLoadContext = new Mock<IGaugeLoadContext>();
        _mockGaugeLoadContext.Setup(x => x.LoadFromAssemblyName(It.Is<AssemblyName>(x => x.FullName == _assemblyName.FullName)))
            .Returns(_mockAssembly.Object);
        _mockGaugeLoadContext.Setup(x => x.LoadFromAssemblyName(It.Is<AssemblyName>(x => x.FullName == libAssemblyName.FullName)))
            .Returns(_mockLibAssembly.Object);
        _mockGaugeLoadContext.Setup(x => x.GetAssembliesReferencingGaugeLib())
            .Returns(new[] { _mockAssembly.Object });
        var mockAssemblyLocator = new Mock<IAssemblyLocater>();
        mockAssemblyLocator.Setup(x => x.GetTestAssembly()).Returns(Path.Combine(_assemblyLocation, "Mock.Test.Assembly.dll"));
        _assemblyLoader = new AssemblyLoader(mockAssemblyLocator.Object, _mockGaugeLoadContext.Object,
            mockReflectionWrapper.Object, mockActivationWrapper.Object, new StepRegistry());
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
    }

    private string _assemblyLocation;

    private AssemblyName _assemblyName;
    private Mock<Assembly> _mockAssembly;
    private Mock<Assembly> _mockLibAssembly;
    private AssemblyLoader _assemblyLoader;
    private Mock<IGaugeLoadContext> _mockGaugeLoadContext;
    private Mock<Type> _mockInstanceManagerType;
    private Mock<Type> _mockScreenshotWriterType;
    private Mock<MethodInfo> _mockStepMethod;
    private const string TmpLocation = "/tmp/location";

    [Test]
    public void ShouldGetAssemblyReferencingGaugeLib()
    {
        ClassicAssert.Contains(_mockAssembly.Object, _assemblyLoader.AssembliesReferencingGaugeLib);
    }

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
        Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TmpLocation);
        var mockReflectionWrapper = new Mock<IReflectionWrapper>();
        var mockActivationWrapper = new Mock<IActivatorWrapper>();
        var mockGaugeLoadContext = new Mock<IGaugeLoadContext>();
        var mockAssemblyLocator = new Mock<IAssemblyLocater>();
        mockAssemblyLocator.Setup(x => x.GetTestAssembly()).Returns(Path.Combine(TmpLocation, $"{_mockLibAssembly.Name}.dll"));
        ClassicAssert.Throws<FileLoadException>(() => new AssemblyLoader(mockAssemblyLocator.Object, mockGaugeLoadContext.Object,
            mockReflectionWrapper.Object, mockActivationWrapper.Object, new StepRegistry()));
    }
}