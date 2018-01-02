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
using Xunit;

namespace Gauge.CSharp.Runner.UnitTests
{
    public class GaugeCommandFactoryTests
    {
        public GaugeCommandFactoryTests()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", Directory.GetCurrentDirectory());
        }

        ~GaugeCommandFactoryTests()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        [Fact]
        public void ShouldGetSetupPhaseExecutorForInit()
        {
            var command = GaugeCommandFactory.GetExecutor("--init");
            Assert.IsType<SetupCommand>(command);
        }

        [Fact]
        public void ShouldGetStartPhaseExecutorByDefault()
        {
            var command = GaugeCommandFactory.GetExecutor(default(string));
            Assert.IsType<StartCommand>(command);
        }
    }
}