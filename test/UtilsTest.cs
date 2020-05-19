/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.IO;
using Gauge.CSharp.Core;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    internal class UtilsTest
    {
        [Test]
        public void ShouldGetCustomBuildPathFromEnvWhenLowerCase()
        {
            Environment.SetEnvironmentVariable("gauge_project_root", @"C:\Blah");

            var imaginaryPath = string.Format("Foo{0}Bar", Path.DirectorySeparatorChar);
            Environment.SetEnvironmentVariable("gauge_custom_build_path", imaginaryPath);
            var gaugeBinDir = Utils.GetGaugeBinDir();
            Assert.AreEqual(string.Format(@"C:\Blah{0}Foo{0}Bar", Path.DirectorySeparatorChar), gaugeBinDir);

            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", string.Empty);
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", string.Empty);
        }

        [Test]
        public void ShouldGetCustomBuildPathFromEnvWhenUpperCase()
        {
            var driveRoot = Path.GetPathRoot(Directory.GetCurrentDirectory());
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Path.Combine(driveRoot, "Blah"));

            var imaginaryPath = Path.Combine("Foo", "Bar");
            ;
            Environment.SetEnvironmentVariable("gauge_custom_build_path", imaginaryPath);
            var gaugeBinDir = Utils.GetGaugeBinDir();
            Assert.AreEqual(Path.Combine(driveRoot, "Blah", "Foo", "Bar"), gaugeBinDir);

            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", string.Empty);
            Environment.SetEnvironmentVariable("GAUGE_CUSTOM_BUILD_PATH", string.Empty);
        }
    }
}