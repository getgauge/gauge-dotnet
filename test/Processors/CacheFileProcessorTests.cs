// Copyright 2019 ThoughtWorks, Inc.
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

using System.Collections.Generic;
using System.Xml.Linq;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Processors
{
    [TestFixture]
    public class CacheFileProcessorTests
    {
        [Test]
        public void ShouldProcessMessage()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(mockAttributesLoader.Object);

            var processor = new CacheFileProcessor(loader);
            var request = new CacheFileRequest
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
            };

            processor.Process(request);

            Assert.True(loader.GetStepRegistry().ContainsStep("goodbye"));
        }

        [Test]
        public void ShouldProcessRequestWithDeleteStatus()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(mockAttributesLoader.Object);
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
            var request = new CacheFileRequest
            {
                FilePath = "Foo.cs",
                Status = CacheFileRequest.Types.FileStatus.Deleted
            };

            Assert.True(loader.GetStepRegistry().ContainsStep("goodbye"));

            processor.Process(request);

            Assert.False(loader.GetStepRegistry().ContainsStep("goodbye"));
        }
    }
}