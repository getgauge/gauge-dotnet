/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using static Gauge.Messages.StepPositionsResponse.Types;

namespace Gauge.Dotnet.UnitTests.Processors;

public class StepPositionsProcessorTests
{
    [Test]
    public async Task ShouldProcessRequest()
    {
        var filePath = "Foo.cs";
        var mockStepRegistry = new Mock<IStepRegistry>();
        mockStepRegistry.Setup(x => x.GetStepPositions(filePath))
            .Returns(new[] { new StepPosition { StepValue = "goodbye", Span = new Span { Start = 6, End = 16 } } });
        var processor = new StepPositionsProcessor(mockStepRegistry.Object);
        var request = new StepPositionsRequest { FilePath = "Foo.cs" };

        var response = await processor.Process(request);

        ClassicAssert.AreEqual(response.StepPositions.Count, 1);
        ClassicAssert.AreEqual(response.StepPositions.First().StepValue, "goodbye");
        ClassicAssert.AreEqual(response.StepPositions.First().Span.Start, 6);
    }


    [Test]
    public async Task ShouldProcessRequestForAliasSteps()
    {
        var filePath = "Foo.cs";
        var mockStepRegistry = new Mock<IStepRegistry>();
        mockStepRegistry.Setup(x => x.GetStepPositions(filePath))
            .Returns(new[] {
                new StepPosition{StepValue = "goodbye", Span = new Span{Start= 6, End= 16}},
                new StepPosition{StepValue = "Sayonara", Span = new Span{Start= 6, End= 16}},
            });
        var processor = new StepPositionsProcessor(mockStepRegistry.Object);
        var request = new StepPositionsRequest { FilePath = filePath };

        var response = await processor.Process(request);

        ClassicAssert.AreEqual(response.StepPositions.Count, 2);
    }
}