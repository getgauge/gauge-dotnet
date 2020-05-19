/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class GaugeCommandFactoryTests
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

        [Test]
        public void ShouldGetSetupPhaseExecutorForInit()
        {
            var command = GaugeCommandFactory.GetExecutor("--init");
            Assert.AreEqual(command.GetType(), typeof(SetupCommand));
        }

        [Test]
        public void ShouldGetStartPhaseExecutorByDefault()
        {
            var command = GaugeCommandFactory.GetExecutor(default(string));
            Assert.AreEqual(command.GetType(), typeof(StartCommand));
        }
    }
}