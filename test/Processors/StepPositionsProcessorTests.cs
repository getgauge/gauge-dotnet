/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using Moq;
using NUnit.Framework;
using static Gauge.Messages.StepPositionsResponse.Types;

namespace Gauge.Dotnet.UnitTests.Processors
{
    public class StepPositionsProcessorTests
    {
        [Test]
        public void ShouldProcessRequest()
        {
            var filePath = "Foo.cs";
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.GetStepPositions(filePath))
                .Returns(new[] {new StepPosition{StepValue = "goodbye", Span = new Span{Start= 6, End= 16}}});
            var processor = new StepPositionsProcessor(mockStepRegistry.Object);
            var request = new StepPositionsRequest {FilePath = "Foo.cs"};

            var response = processor.Process(request);

            Assert.AreEqual(response.StepPositions.Count, 1);
            Assert.AreEqual(response.StepPositions.First().StepValue, "goodbye");
            Assert.AreEqual(response.StepPositions.First().Span.Start, 6);
        }


        [Test]
        public void ShouldProcessRequestForAliasSteps()
        {
            var filePath = "Foo.cs";
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(x => x.GetStepPositions(filePath))
                .Returns(new[] {
                    new StepPosition{StepValue = "goodbye", Span = new Span{Start= 6, End= 16}},
                    new StepPosition{StepValue = "Sayonara", Span = new Span{Start= 6, End= 16}},
                });
            var processor = new StepPositionsProcessor(mockStepRegistry.Object);
            var request = new StepPositionsRequest {FilePath = filePath};

            var response = processor.Process(request);

            Assert.AreEqual(response.StepPositions.Count, 2);
        }
    }
}