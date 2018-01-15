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
using System.Text;
using Gauge.CSharp.Runner.Wrappers;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    public class SandboxTests
    {
        private string _gaugeProjectRootEnv;

        [SetUp]
        public void Setup()
        {
            _gaugeProjectRootEnv = Environment.GetEnvironmentVariable("GAUGE_PROJECT_ROOT");
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());
        }

        [Test]
        public void ShouldLoadScreenGrabber()
        {
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockScreenGrabberType = new Mock<Type>();
            mockAssemblyLoader.Setup(loader => loader.ScreengrabberType)
                .Returns(mockScreenGrabberType.Object);
            var mockActivatorWrapper = new Mock<IActivatorWrapper>();
            var mockScreenGrabber = new Mock<object>();
            var mockReflectionWrapper = new Mock<IReflectionWrapper>();
            mockReflectionWrapper.Setup(x => x.InvokeMethod(mockScreenGrabberType.Object, mockScreenGrabber.Object, "TakeScreenShot"))
                .Returns(Encoding.UTF8.GetBytes("TestScreenGrabber"));
            mockActivatorWrapper.Setup(x => x.CreateInstance(mockScreenGrabberType.Object))
                .Returns(mockScreenGrabber.Object)
                .Verifiable();
            var sandbox = new Sandbox(mockAssemblyLoader.Object, null, mockActivatorWrapper.Object, mockReflectionWrapper.Object);
            byte[] screenshot;
            var tryScreenCapture = sandbox.TryScreenCapture(out screenshot);

            mockActivatorWrapper.VerifyAll();
            Assert.IsTrue(tryScreenCapture);
            Assert.AreEqual("TestScreenGrabber", Encoding.UTF8.GetString(screenshot));
        }

        [Test]
        public void ShouldLoadClassInstanceManager()
        {
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockInstanceManagerType = new Mock<Type>();
            mockAssemblyLoader.Setup(loader => loader.ClassInstanceManagerType)
                .Returns(mockInstanceManagerType.Object);
            var activatorWrapper = new Mock<IActivatorWrapper>();
            var mockInstanceManager = new Mock<object>();
            activatorWrapper.Setup(x => x.CreateInstance(mockInstanceManagerType.Object))
                .Returns(mockInstanceManager.Object)
                .Verifiable();
            var mockTypeWrapper = new Mock<IReflectionWrapper>();
            var sandbox = new Sandbox(mockAssemblyLoader.Object, null, activatorWrapper.Object, mockTypeWrapper.Object);

            activatorWrapper.VerifyAll();
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _gaugeProjectRootEnv);
        }
    }
}