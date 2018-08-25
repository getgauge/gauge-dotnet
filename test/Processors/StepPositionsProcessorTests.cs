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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.Dotnet.UnitTests.Processors
{
    public class StepPositionsProcessorTests
    {
        [Test]
        public void ShouldProcessRequest()
        {
            var xmlMock = new Mock<IXmlLoader>();
            xmlMock.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(xmlMock.Object);
            const string content = "using Gauge.CSharp.Lib.Attributes;\n" +
                                   "namespace foobar\n" +
                                   "{\n" +
                                   "    public class FooBar\n" +
                                   "    {\n" +
                                   "        [Step(\"goodbye\")]\n" +
                                   "        public void farewell()\n" +
                                   "        {\n" +
                                   "        }\n" +
                                   "    }\n" +
                                   "}\n";
            const string file = "Foo.cs";
            loader.LoadStepsFromText(content, file);

            var processor = new StepPositionsProcessor(loader.GetStepRegistry());
            var request = new Message {StepPositionsRequest = new StepPositionsRequest {FilePath = "Foo.cs"}};

            var response = processor.Process(request).StepPositionsResponse;

            Assert.AreEqual(response.StepPositions.Count, 1);
            Assert.AreEqual(response.StepPositions.First().StepValue, "goodbye");
            Assert.AreEqual(response.StepPositions.First().Span.Start, 6);
        }


        [Test]
        public void ShouldProcessRequestForAliasSteps()
        {
            var xmlMock = new Mock<IXmlLoader>();
            xmlMock.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(xmlMock.Object);
            const string content = "using Gauge.CSharp.Lib.Attributes;\n" +
                                   "namespace foobar\n" +
                                   "{\n" +
                                   "    public class FooBar\n" +
                                   "    {\n" +
                                   "        [Step(\"goodbye\",\"sayonara\")]\n" +
                                   "        public void farewell()\n" +
                                   "        {\n" +
                                   "        }\n" +
                                   "    }\n" +
                                   "}\n";
            const string file = "Foo.cs";
            loader.LoadStepsFromText(content, file);

            var processor = new StepPositionsProcessor(loader.GetStepRegistry());
            var request = new Message {StepPositionsRequest = new StepPositionsRequest {FilePath = "Foo.cs"}};

            var response = processor.Process(request).StepPositionsResponse;

            Assert.AreEqual(response.StepPositions.Count, 2);
        }
    }
}