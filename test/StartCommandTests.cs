/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    internal class StartCommandTests
    {
        class FakeGaugeListener : GaugeListener
        {
            public FakeGaugeListener(IConfiguration configuration) : base(configuration)
            {
            }

            public override void ConfigureServices(IServiceCollection services) {}
            public override void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime) {}
        }

        [SetUp]
        public void Setup()
        {
            _mockGaugeProjectBuilder = new Mock<IGaugeProjectBuilder>();
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TempPath);
            _startCommand = new StartCommand(_mockGaugeProjectBuilder.Object, typeof(FakeGaugeListener));
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", null);
        }

        private readonly string TempPath = Path.GetTempPath();

        private Mock<IGaugeProjectBuilder> _mockGaugeProjectBuilder;
        private StartCommand _startCommand;

        [Test]
        public void ShouldInvokeProjectBuild()
        {
            _startCommand.Execute().ContinueWith(b => {
                _mockGaugeProjectBuilder.Verify(builder => builder.BuildTargetGaugeProject(), Times.Once);
            });
        }

        [Test]
        public void ShouldNotBuildWhenCustomBuildPathIsSetAsync()
        {
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", "GAUGE_CUSTOM_BUILD_PATH");
            _startCommand.Execute().ContinueWith(b => {
                _mockGaugeProjectBuilder.Verify(builder => builder.BuildTargetGaugeProject(), Times.Never);
            });

        }

        [Test]
        public void ShouldNotPollForMessagesWhenBuildFails()
        {
            _mockGaugeProjectBuilder.Setup(builder => builder.BuildTargetGaugeProject()).Returns(false);
            _startCommand.Execute()
                .ContinueWith(b => Assert.False(b.Result, "Should not start server when build fails"));
        }

        [Test]
        public void ShouldPollForMessagesWhenBuildPasses()
        {
            _mockGaugeProjectBuilder.Setup(builder => builder.BuildTargetGaugeProject()).Returns(true);

            _startCommand.Execute()
                .ContinueWith(b => Assert.True(b.Result, "Should start server using GaugeListener when build passes"));
        }

        [Test]
        public void ShouldPollForMessagesWhenCustomBuildPathIsSet()
        {
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", "GAUGE_CUSTOM_BUILD_PATH");

            _startCommand.Execute()
                .ContinueWith(b => Assert.True(b.Result, "Should start server using GaugeListener when GAUGE_CUSTOM_BUILD_PATH is set"));
        }

        [Test]
        public void ShouldRunProcessInProjectRoot()
        {
            var actual = Environment.CurrentDirectory.TrimEnd(Path.DirectorySeparatorChar);
            var expected = TempPath.TrimEnd(Path.DirectorySeparatorChar);
            // in osx the /var tmp path is a symlink to /private/var
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                expected = $"/private{expected}";

            Assert.That(actual, Is.SamePath(expected));
        }
    }
}