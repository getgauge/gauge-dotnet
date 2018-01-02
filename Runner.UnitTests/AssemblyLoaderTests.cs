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
using System.Reflection;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Wrappers;
using Moq;
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests
{
    public class AssemblyLoaderTests
    {
        public AssemblyLoaderTests()
        {
            var libPath = Path.GetFullPath(Path.Combine(TmpLocation, "gauge-bin", "Gauge.CSharp.Lib.dll"));
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TmpLocation);
            var thisType = GetType();
            var assemblyLocation = thisType.Assembly.Location;
            _mockAssemblyWrapper = new Mock<IAssemblyWrapper>();
            _stepMethod = thisType.GetMethod("DummyStepMethod");
            _mockAssembly = new Mock<TestAssembly>();
            _mockInstanceManagerType = new Mock<Type>();
            _mockInstanceManagerType.Setup(type => type.GetInterfaces()).Returns(new[] {typeof(IClassInstanceManager)});
            _mockAssembly.Setup(assembly => assembly.GetTypes())
                .Returns(new[] {thisType, _mockInstanceManagerType.Object});
            _mockAssembly.Setup(assembly => assembly.GetType(thisType.FullName)).Returns(thisType);
            _mockAssembly.Setup(assembly => assembly.GetType(_mockInstanceManagerType.Object.FullName))
                .Returns(_mockInstanceManagerType.Object);
            _mockAssembly.Setup(assembly => assembly.GetReferencedAssemblies())
                .Returns(new[] {new AssemblyName("Gauge.CSharp.Lib")});
            _mockAssemblyWrapper.Setup(wrapper => wrapper.LoadFrom(libPath)).Returns(typeof(Step).Assembly)
                .Verifiable();
            _mockAssemblyWrapper.Setup(wrapper => wrapper.LoadFrom(assemblyLocation)).Returns(_mockAssembly.Object);
            _assemblyLoader = new AssemblyLoader(_mockAssemblyWrapper.Object, new[] { assemblyLocation });
        }

        ~AssemblyLoaderTests()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        [Step("Foo text")]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void DummyStepMethod()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
        }

        private Mock<TestAssembly> _mockAssembly;
        private MethodInfo _stepMethod;
        private AssemblyLoader _assemblyLoader;
        private Mock<IAssemblyWrapper> _mockAssemblyWrapper;
        private Mock<Type> _mockInstanceManagerType;
        private const string TmpLocation = "/tmp/location";

        [Fact]
        public void ShouldGetAssemblyReferencingGaugeLib()
        {
            Assert.Contains(_mockAssembly.Object, _assemblyLoader.AssembliesReferencingGaugeLib);
        }

        [Fact]
        public void ShouldGetClassInstanceManagerTypes()
        {
            Assert.Contains(_mockInstanceManagerType.Object, _assemblyLoader.ClassInstanceManagerTypes);
        }

        [Fact]
        public void ShouldGetMethodsForGaugeAttribute()
        {
            Assert.Contains(_stepMethod, _assemblyLoader.GetMethods("Gauge.CSharp.Lib.Attribute.Step"));
        }

        [Fact]
        public void ShouldGetTargetAssembly()
        {
            _mockAssemblyWrapper.VerifyAll();
        }

        [Fact]
        public void ShouldThrowExceptionWhenLibAssemblyNotFound()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TmpLocation);
            var mockAssemblyWrapper = new Mock<IAssemblyWrapper>();

            Assert.Throws<FileNotFoundException>(() =>
                new AssemblyLoader(mockAssemblyWrapper.Object, new[] { TmpLocation }));
        }
    }
}