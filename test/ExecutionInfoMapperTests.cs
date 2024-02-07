using NUnit.Framework;
using Moq;
using Gauge.Messages;
using Gauge.Dotnet.Wrappers;
using Gauge.CSharp.Lib;
using System.Linq;

namespace Gauge.Dotnet.UnitTests
{
    [TestFixture]
    public class ExecutionInfoMapperTests
    {
        private ExecutionInfo executionInfo;
        private Mock<IAssemblyLoader> mockAssemblyLoader;

        [SetUp]
        public void Setup()
        {
            mockAssemblyLoader = new Mock<IAssemblyLoader>();
            mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ExecutionContext)).Returns(typeof(ExecutionContext));
            executionInfo = new ExecutionInfo {
                CurrentScenario = new ScenarioInfo{IsFailed = true, Name = "Dummy Scenario" },
                CurrentSpec = new SpecInfo {FileName = "dummy.spec", Name = "Dummy Spec", IsFailed = true},
                CurrentStep = new StepInfo {IsFailed = true, ErrorMessage = "Dummy Error", StackTrace = "Dummy Stacktrace", 
                    Step = new ExecuteStepRequest {ActualStepText = "Dummy Step Text"}
                }
            };
        }

        [Test]
        public void ShouldMapSpecInfo()
        {
            var mockActivatorWrapper = new Mock<IActivatorWrapper>();
            mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(ExecutionContext.Specification),
                executionInfo.CurrentSpec.Name, executionInfo.CurrentSpec.FileName, executionInfo.CurrentSpec.IsFailed,
                executionInfo.CurrentSpec.Tags.ToArray())).Verifiable();
            new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
            mockActivatorWrapper.VerifyAll();
        }

        [Test]
        public void ShouldMapNullSpecInfo()
        {
            executionInfo.CurrentSpec = null;
            var mockActivatorWrapper = new Mock<IActivatorWrapper>();
            mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(ExecutionContext.Specification))).Verifiable();
            new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
            mockActivatorWrapper.VerifyAll();
        }

        [Test]
        public void ShouldMapScenarioInfo()
        {
            var mockActivatorWrapper = new Mock<IActivatorWrapper>();
            mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(ExecutionContext.Scenario),
                executionInfo.CurrentScenario.Name, executionInfo.CurrentScenario.IsFailed,
                executionInfo.CurrentScenario.Tags.ToArray())).Verifiable();
            new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
            mockActivatorWrapper.VerifyAll();
        }

        [Test]
        public void ShouldMapNullScenarioInfo()
        {
            executionInfo.CurrentScenario = null;
            var mockActivatorWrapper = new Mock<IActivatorWrapper>();
            mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(ExecutionContext.Scenario))).Verifiable();
            new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
            mockActivatorWrapper.VerifyAll();
        }

        [Test]
        public void ShouldMapStepDetails()
        {
            var mockActivatorWrapper = new Mock<IActivatorWrapper>();
            mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(ExecutionContext.StepDetails),
                executionInfo.CurrentStep.Step.ActualStepText, executionInfo.CurrentStep.IsFailed, executionInfo.CurrentStep.Step.ParsedStepText,
                executionInfo.CurrentStep.StackTrace, executionInfo.CurrentStep.ErrorMessage, executionInfo.CurrentStep.IsConcept)).Verifiable();
            new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
            mockActivatorWrapper.VerifyAll();
        }

        [Test]
        public void ShouldMapNullStepDetails()
        {
            executionInfo.CurrentStep = null;
            var mockActivatorWrapper = new Mock<IActivatorWrapper>();
            mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(ExecutionContext.StepDetails))).Verifiable();
            new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
            mockActivatorWrapper.VerifyAll();
        }    
    }
}