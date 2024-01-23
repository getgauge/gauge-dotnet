using System.Collections.Generic;
using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Gauge.Dotnet.UnitTests.Processors
{
    public class StepNamesProcessorTests
    {
        [Test]
        public void ShouldProcessStepNamesRequest()
        {
            var mockStepRegistry = new Mock<IStepRegistry>();
            mockStepRegistry.Setup(r => r.GetStepTexts()).Returns(new List<string> { "step1", "step2", "step3" });
            var stepNamesProcessor = new StepNamesProcessor(mockStepRegistry.Object);
            var request = new StepNamesRequest();
            var response = stepNamesProcessor.Process(request);
            ClassicAssert.AreEqual(3, response.Steps.Count);
            ClassicAssert.AreEqual(response.Steps[0], "step1");
        }
    }
}