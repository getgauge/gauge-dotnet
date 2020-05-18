/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class AssemblyLoaderTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TmpLocation);
            var assemblyLocation = "/foo/location";
            _mockAssembly = new Mock<Assembly>();
            _mockAssemblyWrapper = new Mock<IAssemblyWrapper>();
            var mockActivationWrapper = new Mock<IActivatorWrapper>();
            var mockType = new Mock<Type>();
            _mockStepMethod = new Mock<MethodInfo>();
            var mockStepAttribute = new Mock<Attribute>();
            _mockStepMethod.Setup(x => x.GetCustomAttributes(false))
                .Returns(new[] {mockStepAttribute.Object});
            mockType.Setup(x => x.IsInstanceOfType(mockStepAttribute.Object))
                .Returns(true);
            var mockIClassInstanceManagerType = new Mock<Type>();
            mockIClassInstanceManagerType.Setup(x => x.FullName).Returns("Gauge.CSharp.Lib.IClassInstanceManager");
            _mockInstanceManagerType = new Mock<Type>();
            _mockInstanceManagerType.Setup(type => type.GetInterfaces())
                .Returns(new[] {mockIClassInstanceManagerType.Object});
            _mockInstanceManagerType.Setup(x => x.Name)
                .Returns("TestInstanceManager");

            var mockIScreenshotWriter = new Mock<Type>();
            mockIScreenshotWriter.Setup(x => x.FullName).Returns("Gauge.CSharp.Lib.ICustomScreenshotWriter");
            _mockScreenshotWriter = new Mock<Type>();
            _mockScreenshotWriter.Setup(x => x.Name)
                .Returns("TestScreenGrabber");
            _mockScreenshotWriter.Setup(x => x.GetInterfaces())
                .Returns(new[] {mockIScreenshotWriter.Object});
            var assemblyName = new AssemblyName("Gauge.CSharp.Lib");
            _mockAssembly.Setup(assembly => assembly.GetTypes())
                .Returns(new[]
                {
                    mockType.Object,
                    _mockScreenshotWriter.Object,
                    _mockInstanceManagerType.Object
                });
            _mockAssembly.Setup(x => x.GetName())
                .Returns(assemblyName);
            _mockAssembly.Setup(assembly => assembly.GetType(_mockScreenshotWriter.Object.FullName))
                .Returns(_mockScreenshotWriter.Object);
            _mockAssembly.Setup(assembly => assembly.GetType(_mockInstanceManagerType.Object.FullName))
                .Returns(_mockInstanceManagerType.Object);
            _mockAssembly.Setup(assembly => assembly.GetType(LibType.Step.FullName()))
                .Returns(mockType.Object);
            _mockAssembly.Setup(assembly => assembly.GetReferencedAssemblies())
                .Returns(new[] {assemblyName});
            _mockAssemblyWrapper.Setup(x => x.LoadFrom(assemblyLocation))
                .Returns(_mockAssembly.Object);
            _mockAssemblyWrapper.Setup(x => x.GetCurrentDomainAssemblies())
                .Returns(new[] {_mockAssembly.Object});
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            mockReflectionWrapper.Setup(r => r.GetMethods(mockType.Object))
                .Returns(new[] {_mockStepMethod.Object});
            _assemblyLoader = new AssemblyLoader(_mockAssemblyWrapper.Object, new[] {assemblyLocation},
                mockReflectionWrapper.Object, mockActivationWrapper.Object, new StepRegistry());
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        private Mock<Assembly> _mockAssembly;
        private AssemblyLoader _assemblyLoader;
        private Mock<IAssemblyWrapper> _mockAssemblyWrapper;
        private Mock<Type> _mockInstanceManagerType;
        private Mock<Type> _mockScreenshotWriter;
        private Mock<MethodInfo> _mockStepMethod;
        private const string TmpLocation = "/tmp/location";

        [Test]
        public void ShouldGetAssemblyReferencingGaugeLib()
        {
            Assert.Contains(_mockAssembly.Object, _assemblyLoader.AssembliesReferencingGaugeLib);
        }

        [Test]
        public void ShouldGetClassInstanceManagerType()
        {
            Assert.AreEqual(_mockInstanceManagerType.Object.Name, _assemblyLoader.ClassInstanceManagerType.Name);
        }

        [Test]
        public void ShouldGetMethodsForGaugeAttribute()
        {
            Assert.Contains(_mockStepMethod.Object, _assemblyLoader.GetMethods(LibType.Step).ToList());
        }

        [Test]
        public void ShouldGetScreenGrabberType()
        {
            Assert.AreEqual(_mockScreenshotWriter.Object.Name, _assemblyLoader.ScreenshotWriter.Name);
        }

        [Test]
        public void ShouldGetTargetAssembly()
        {
            _mockAssemblyWrapper.VerifyAll();
        }

        [Test]
        public void ShouldThrowExceptionWhenLibAssemblyNotFound()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TmpLocation);
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            var mockAssemblyWrapper = new Mock<IAssemblyWrapper>();
            var mockActivationWrapper = new Mock<IActivatorWrapper>();
            mockAssemblyWrapper.Setup(x => x.LoadFrom(TmpLocation)).Throws<FileNotFoundException>();
            Assert.Throws<FileNotFoundException>(() =>
                new AssemblyLoader(mockAssemblyWrapper.Object, new[] {TmpLocation}, mockReflectionWrapper.Object, mockActivationWrapper.Object, new StepRegistry()));
        }
    }
}