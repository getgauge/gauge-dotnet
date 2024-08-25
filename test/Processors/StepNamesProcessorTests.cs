using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;

namespace Gauge.Dotnet.UnitTests.Processors;

public class StepNamesProcessorTests
{
    [Test]
    public async Task ShouldProcessStepNamesRequest()
    {
        var mockStepRegistry = new Mock<IStepRegistry>();
        mockStepRegistry.Setup(r => r.GetStepTexts()).Returns(new List<string> { "step1", "step2", "step3" });
        var stepNamesProcessor = new StepNamesProcessor(mockStepRegistry.Object);
        var request = new StepNamesRequest();
        var response = await stepNamesProcessor.Process(request);
        ClassicAssert.AreEqual(3, response.Steps.Count);
        ClassicAssert.AreEqual(response.Steps[0], "step1");
    }
}