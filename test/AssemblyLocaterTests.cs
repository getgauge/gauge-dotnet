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
using Gauge.Dotnet.Exceptions;
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
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        private readonly Mock<IDirectoryWrapper> _mockDirectoryWrapper = new Mock<IDirectoryWrapper>();

        [Test]
        public void ShouldGetAssembliesFromGaugeBin()
        {
            var expected = "fooAssemblyLocation";
            var expectedAssemblies = new[] {$"{expected}.deps.json"};
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.deps.json", SearchOption.TopDirectoryOnly))
                .Returns(expectedAssemblies);
            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object);

            var assembly = assemblyLocater.GetTestAssembly();

            Assert.AreEqual($"{expected}.dll", assembly);
        }

        [Test]
        public void ShouldNotAddAssembliesFromInvalidFile()
        {
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(Utils.GetGaugeBinDir(), "*.deps.json", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());

            var expectedMessage = $"Could not locate the target test assembly. Gauge-Dotnet could not find a deps.json file in {Directory.GetCurrentDirectory()}";
            Assert.Throws<GaugeTestAssemblyNotFoundException>(() => new AssemblyLocater(_mockDirectoryWrapper.Object).GetTestAssembly(), expectedMessage);
        }
    }
}