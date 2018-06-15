// Copyright 2015 ThoughtWorks, Inc.
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
using System.IO;
using System.Linq;
using System.Reflection;
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

            var mockIScreenGrabberType = new Mock<Type>();
            mockIScreenGrabberType.Setup(x => x.FullName).Returns("Gauge.CSharp.Lib.ICustomScreenshotGrabber");
            _mockScreenGrabberType = new Mock<Type>();
            _mockScreenGrabberType.Setup(x => x.Name)
                .Returns("TestScreenGrabber");
            _mockScreenGrabberType.Setup(x => x.GetInterfaces())
                .Returns(new[] {mockIScreenGrabberType.Object});
            var assemblyName = new AssemblyName("Gauge.CSharp.Lib");
            _mockAssembly.Setup(assembly => assembly.GetTypes())
                .Returns(new[]
                {
                    mockType.Object,
                    _mockScreenGrabberType.Object,
                    _mockInstanceManagerType.Object
                });
            _mockAssembly.Setup(x => x.GetName())
                .Returns(assemblyName);
            _mockAssembly.Setup(assembly => assembly.GetType(_mockScreenGrabberType.Object.FullName))
                .Returns(_mockScreenGrabberType.Object);
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
                mockReflectionWrapper.Object);
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
        private Mock<Type> _mockScreenGrabberType;
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
            Assert.AreEqual(_mockScreenGrabberType.Object.Name, _assemblyLoader.ScreengrabberType.Name);
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
            mockAssemblyWrapper.Setup(x => x.LoadFrom(TmpLocation)).Throws<FileNotFoundException>();
            Assert.Throws<FileNotFoundException>(() =>
                new AssemblyLoader(mockAssemblyWrapper.Object, new[] {TmpLocation}, mockReflectionWrapper.Object));
        }
    }
}