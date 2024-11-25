/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Exceptions;
using Gauge.Dotnet.Extensions;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Wrappers;
using Microsoft.Extensions.Configuration;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    internal class AssemblyLocaterTests
    {
        private readonly Mock<IDirectoryWrapper> _mockDirectoryWrapper = new Mock<IDirectoryWrapper>();
        private string _rootDirectory;
        private IConfiguration _config;

        [SetUp]
        public void Setup()
        {
            _rootDirectory = Directory.GetCurrentDirectory();
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "GAUGE_PROJECT_ROOT", _rootDirectory } })
                .Build();
        }

        [Test]
        public void ShouldGetAssembliesFromGaugeBin()
        {
            var expected = "fooAssemblyLocation";
            var expectedAssemblies = new[] { $"{expected}.deps.json" };
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(_config.GetGaugeBinDir(), "*.deps.json", SearchOption.TopDirectoryOnly))
                .Returns(expectedAssemblies);
            var assemblyLocater = new AssemblyLocater(_mockDirectoryWrapper.Object, _config);

            var assembly = assemblyLocater.GetTestAssembly();

            ClassicAssert.AreEqual($"{expected}.dll", (string)assembly);
        }

        [Test]
        public void ShouldNotAddAssembliesFromInvalidFile()
        {
            _mockDirectoryWrapper.Setup(wrapper =>
                    wrapper.EnumerateFiles(_config.GetGaugeBinDir(), "*.deps.json", SearchOption.TopDirectoryOnly))
                .Returns(Enumerable.Empty<string>());

            var expectedMessage = $"Could not locate the target test assembly. Gauge-Dotnet could not find a deps.json file in {Directory.GetCurrentDirectory()}";
            ClassicAssert.Throws<GaugeTestAssemblyNotFoundException>(() => new AssemblyLocater(_mockDirectoryWrapper.Object, _config).GetTestAssembly(), expectedMessage);
        }
    }
}