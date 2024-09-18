using Gauge.Dotnet.Models;
using Gauge.Dotnet.Processors;
using Gauge.Messages;

namespace Gauge.Dotnet.UnitTests.Processors;

public class StepNameProcessorTest
{
    public class StepNameProcessorTests
    {
        [Test]
        public async Task ShouldProcessStepNameRequest()
        {
            var mockStepRegistry = new Mock<IStepRegistry>();
            var request = new StepNameRequest
            {
                StepValue = "step1"
            };

            var parsedStepText = request.StepValue;
            const string stepText = "step1";
            mockStepRegistry.Setup(r => r.ContainsStep(parsedStepText)).Returns(true);
            mockStepRegistry.Setup(r => r.GetStepText(parsedStepText)).Returns(stepText);
            var gaugeMethod = new GaugeMethod
            {
                FileName = "foo"
            };
            mockStepRegistry.Setup(r => r.MethodFor(parsedStepText)).Returns(gaugeMethod);
            mockStepRegistry.Setup(r => r.HasAlias(stepText)).Returns(false);
            var stepNameProcessor = new StepNameProcessor(mockStepRegistry.Object);

            var response = await stepNameProcessor.Process(1, request);

            ClassicAssert.AreEqual(response.FileName, "foo");
            ClassicAssert.AreEqual(response.StepName[0], "step1");
            ClassicAssert.False(response.HasAlias);
        }

        [Test]
        public async Task ShouldProcessStepNameWithAliasRequest()
        {
            var mockStepRegistry = new Mock<IStepRegistry>();
            var request = new StepNameRequest
            {
                StepValue = "step1"
            };
            var parsedStepText = request.StepValue;
            const string stepText = "step1";
            mockStepRegistry.Setup(r => r.ContainsStep(parsedStepText)).Returns(true);
            mockStepRegistry.Setup(r => r.GetStepText(parsedStepText)).Returns(stepText);

            var gaugeMethod = new GaugeMethod
            {
                FileName = "foo",
                HasAlias = true,
                Aliases = new List<string> { "step2", "step3" }
            };
            mockStepRegistry.Setup(r => r.MethodFor(parsedStepText)).Returns(gaugeMethod);
            mockStepRegistry.Setup(r => r.HasAlias(stepText)).Returns(true);
            var stepNameProcessor = new StepNameProcessor(mockStepRegistry.Object);

            var response = await stepNameProcessor.Process(1, request);

            ClassicAssert.AreEqual(response.FileName, "foo");
            ClassicAssert.AreEqual(response.StepName[0], "step2");
            ClassicAssert.AreEqual(response.StepName[1], "step3");
            ClassicAssert.True(response.HasAlias);
        }

        [Test]
        public async Task ShouldProcessExternalSteps()
        {
            var mockStepRegistry = new Mock<IStepRegistry>();
            var request = new StepNameRequest
            {
                StepValue = "step1"
            };
            var parsedStepText = request.StepValue;
            const string stepText = "step1";
            mockStepRegistry.Setup(r => r.ContainsStep(parsedStepText)).Returns(true);
            mockStepRegistry.Setup(r => r.GetStepText(parsedStepText)).Returns(stepText);

            var gaugeMethod = new GaugeMethod
            {
                FileName = "foo",
                IsExternal = true
            };
            mockStepRegistry.Setup(r => r.MethodFor(parsedStepText)).Returns(gaugeMethod);
            var stepNameProcessor = new StepNameProcessor(mockStepRegistry.Object);

            var response = await stepNameProcessor.Process(1, request);

            ClassicAssert.True(response.IsExternal);
            // ClassicAssert.AreEqual(response.FileName, null);
        }
    }
}