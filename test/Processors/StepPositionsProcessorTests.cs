/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
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
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(new Lazy<IAttributesLoader>(() => mockAttributesLoader.Object));
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
            var request = new StepPositionsRequest {FilePath = "Foo.cs"};

            var response = processor.Process(request);

            Assert.AreEqual(response.StepPositions.Count, 1);
            Assert.AreEqual(response.StepPositions.First().StepValue, "goodbye");
            Assert.AreEqual(response.StepPositions.First().Span.Start, 6);
        }


        [Test]
        public void ShouldProcessRequestForAliasSteps()
        {
            var mockAttributesLoader = new Mock<IAttributesLoader>();
            mockAttributesLoader.Setup(x => x.GetRemovedAttributes()).Returns(new List<XAttribute>());
            var loader = new StaticLoader(new Lazy<IAttributesLoader>(() => mockAttributesLoader.Object));
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
            var request = new StepPositionsRequest {FilePath = "Foo.cs"};

            var response = processor.Process(request);

            Assert.AreEqual(response.StepPositions.Count, 2);
        }
    }
}