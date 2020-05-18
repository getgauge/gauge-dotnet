/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using System.Runtime.InteropServices;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    internal class StartCommandTests
    {
        [SetUp]
        public void Setup()
        {
            _mockGaugeListener = new Mock<IGaugeListener>();
            _mockGaugeProjectBuilder = new Mock<IGaugeProjectBuilder>();
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", TempPath);
            _startCommand = new StartCommand(() => _mockGaugeListener.Object, () => _mockGaugeProjectBuilder.Object);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", null);
        }

        private readonly string TempPath = Path.GetTempPath();

        private Mock<IGaugeListener> _mockGaugeListener;
        private Mock<IGaugeProjectBuilder> _mockGaugeProjectBuilder;
        private StartCommand _startCommand;

        [Test]
        public void ShouldInvokeProjectBuild()
        {
            _startCommand.Execute();

            _mockGaugeProjectBuilder.Verify(builder => builder.BuildTargetGaugeProject(), Times.Once);
        }

        [Test]
        public void ShouldNotBuildWhenCustomBuildPathIsSet()
        {
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", "GAUGE_CUSTOM_BUILD_PATH");
            _startCommand.Execute();

            _mockGaugeProjectBuilder.Verify(builder => builder.BuildTargetGaugeProject(), Times.Never);
        }

        [Test]
        public void ShouldNotPollForMessagesWhenBuildFails()
        {
            _mockGaugeProjectBuilder.Setup(builder => builder.BuildTargetGaugeProject()).Returns(false);

            _startCommand.Execute();

            _mockGaugeListener.Verify(listener => listener.StartServer(), Times.Never);
        }

        [Test]
        public void ShouldPollForMessagesWhenBuildPasses()
        {
            _mockGaugeProjectBuilder.Setup(builder => builder.BuildTargetGaugeProject()).Returns(true);

            _startCommand.Execute();

            _mockGaugeListener.Verify(listener => listener.StartServer(), Times.Once);
        }

        [Test]
        public void ShouldPollForMessagesWhenCustomBuildPathIsSet()
        {
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", "GAUGE_CUSTOM_BUILD_PATH");
            _startCommand.Execute();

            _mockGaugeListener.Verify(listener => listener.StartServer(), Times.Once);
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