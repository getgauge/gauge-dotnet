// Copyright 2018 ThoughtWorks, Inc.
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

using Gauge.Dotnet.Processors;
using Gauge.Messages;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Processors
{
    [TestFixture]
    public class CacheFileProcessorTests
    {
        [Test]
        public void ShouldProcessMessage()
        {
            var loader = new StaticLoader();

            var processor = new CacheFileProcessor(loader);
            var request = new Message
            {
                CacheFileRequest = new CacheFileRequest
                {
                    Content = "using Gauge.CSharp.Lib.Attributes;\n" +
                              "namespace foobar\n" +
                              "{\n" +
                              "    public class FooBar\n" +
                              "    {\n" +
                              "        [Step(\"goodbye\",\"adieu\", \"sayonara\")]\n" +
                              "        public void farewell()\n" +
                              "        {\n" +
                              "        }\n" +
                              "    }\n" +
                              "}\n",
                    FilePath = "Foo.cs",
                    Status = CacheFileRequest.Types.FileStatus.Opened
                }
            };

            processor.Process(request);

            Assert.True(loader.GetStepRegistry().ContainsStep("goodbye"));
        }

        [Test]
        public void ShouldProcessRequestWithDeleteStatus()
        {
            var loader = new StaticLoader();
            const string content = "using Gauge.CSharp.Lib.Attributes;\n" +
                                   "namespace foobar\n" +
                                   "{\n" +
                                   "    public class FooBar\n" +
                                   "    {\n" +
                                   "        [Step(\"goodbye\",\"adieu\", \"sayonara\")]\n" +
                                   "        public void farewell()\n" +
                                   "        {\n" +
                                   "        }\n" +
                                   "    }\n" +
                                   "}\n";
            loader.LoadStepsFromText(content, "Foo.cs");

            var processor = new CacheFileProcessor(loader);
            var request = new Message
            {
                CacheFileRequest = new CacheFileRequest
                {
                    FilePath = "Foo.cs",
                    Status = CacheFileRequest.Types.FileStatus.Deleted
                }
            };

            Assert.True(loader.GetStepRegistry().ContainsStep("goodbye"));

            processor.Process(request);

            Assert.False(loader.GetStepRegistry().ContainsStep("goodbye"));
        }
    }
}