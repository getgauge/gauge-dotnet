// Copyright 2018 ThoughtWorks, Inc.
//
// This file is part of Gauge-Dotnet.
//
// Gauge-Dotnet is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-Dotnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-Dotnet.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;

namespace Gauge.Dotnet.IntegrationTests
{
    internal class TestUtils
    {
        public static string GetIntegrationTestSampleDirectory()
        {
            /* Rather than assuming what directory integration tests are executing from
             * We will discover 'up' the IntegrationTestSample project
            */
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            System.Console.WriteLine(dir.FullName);
            return FindIntegrationTestDirectory(dir).FullName;
        }

        private static DirectoryInfo FindIntegrationTestDirectory(DirectoryInfo dir)
        {
            var testdataDir = dir.GetDirectories().FirstOrDefault(d => d.Name.Equals("_testdata"));
            if (testdataDir != null){
                return testdataDir.GetDirectories().First(d => d.Name.Equals("Sample"));
            }
            if (dir.Parent != null) // not on system boundry
                return FindIntegrationTestDirectory(dir.Parent);
            throw new DirectoryNotFoundException("Failed to find Sample directory");
        }
    }
}