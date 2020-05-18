/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using System.Linq;
using Gauge.CSharp.Core;
using Gauge.Dotnet.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    internal class AssemblyLocaterTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", null);
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        private readonly Mock<IDirectoryWrapper> _mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        private readonly Mock<IFileWrapper> _mockFileWrapper = new Mock<IFileWrapper>();

        [Test]
        public void ShouldAddAssembliesFromMultipleLocations()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo.dll, foo/");
            var expectedAssemblies = new[] {Path.GetFullPath("foo.dll"), "fooAssemblyLocation", "barAssemblyLocation"};
            _mockDirectoryWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo/"))).Returns(true);
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Path.GetFullPath("foo/"), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(new[] {"fooAssemblyLocation", "barAssemblyLocation"});
            _mockFileWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo.dll"))).Returns(true);
            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();

            Assert.AreEqual(expectedAssemblies, assemblies);
        }

        [Test]
        public void ShouldAddAssembliyFromGaugeAdditionalLibFile()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo.dll");
            var expectedAssemblies = new[] {Path.GetFullPath("foo.dll")};
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());
            _mockFileWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo.dll"))).Returns(true);
            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();
            Assert.AreEqual(expectedAssemblies, assemblies);
        }

        [Test]
        public void ShouldGetAssembliesFromGaugeAdditionalLibsEnvVar()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo/");
            var expectedAssemblies = new[] {"fooAssemblyLocation", "barAssemblyLocation"};
            _mockDirectoryWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo/"))).Returns(true);
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Path.GetFullPath("foo/"), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(expectedAssemblies);

            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();
            Assert.AreEqual(expectedAssemblies, assemblies);
        }

        [Test]
        public void ShouldGetAssembliesFromGaugeBin()
        {
            var expectedAssemblies = new[] {"fooAssemblyLocation", "barAssemblyLocation"};
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(expectedAssemblies);
            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();

            Assert.AreEqual(expectedAssemblies, assemblies);
        }

        [Test]
        public void ShouldNotAddAssembliesFromInvalidFile()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo.dll");
            _mockFileWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo.dll"))).Returns(false);
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());

            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();
            Assert.IsEmpty(assemblies);
        }

        [Test]
        public void ShoulNotdAddAssembliesFromInvalidDirectory()
        {
            Environment.SetEnvironmentVariable("GAUGE_ADDITIONAL_LIBS", "foo/");
            _mockDirectoryWrapper.Setup(wrapper => wrapper.Exists(Path.GetFullPath("foo/"))).Returns(false);
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.dll", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());

            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _mockFileWrapper.Object);

            var assemblies = assemblyLocater.GetAllAssemblies();
            Assert.IsEmpty(assemblies);
        }
    }
}