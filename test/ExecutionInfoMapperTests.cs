using Gauge.Dotnet.Wrappers;
using Gauge.Messages;

namespace Gauge.Dotnet.UnitTests;

[TestFixture]
public class ExecutionInfoMapperTests
{
    private ExecutionInfo executionInfo;
    private Mock<IAssemblyLoader> mockAssemblyLoader;

    [SetUp]
    public void Setup()
    {
        mockAssemblyLoader = new Mock<IAssemblyLoader>();
        mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ExecutionContext)).Returns(typeof(CSharp.Lib.ExecutionContext));
        executionInfo = new ExecutionInfo
        {
            CurrentScenario = new ScenarioInfo
            {
                IsFailed = true,
                Name = "Dummy Scenario",
                Retries = new ScenarioRetriesInfo { MaxRetries = 0, CurrentRetry = 0 }
            },
            CurrentSpec = new SpecInfo { FileName = "dummy.spec", Name = "Dummy Spec", IsFailed = true },
            CurrentStep = new StepInfo
            {
                IsFailed = true,
                ErrorMessage = "Dummy Error",
                StackTrace = "Dummy Stacktrace",
                Step = new ExecuteStepRequest { ActualStepText = "Dummy Step Text" }
            }
        };
    }

    [Test]
    public void ShouldMapSpecInfo()
    {
        var mockActivatorWrapper = new Mock<IActivatorWrapper>();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.Specification),
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
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.Specification))).Verifiable();
        new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
        mockActivatorWrapper.VerifyAll();
    }

    [Test]
    public void ShouldMapScenarioInfo()
    {
        var mockActivatorWrapper = new Mock<IActivatorWrapper>();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.Scenario),
            executionInfo.CurrentScenario.Name, executionInfo.CurrentScenario.IsFailed,
            executionInfo.CurrentScenario.Tags.ToArray(),
            executionInfo.CurrentScenario.Retries.MaxRetries, executionInfo.CurrentScenario.Retries.CurrentRetry)).Verifiable();
        new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
        mockActivatorWrapper.VerifyAll();
    }

    [Test]
    public void ShouldMapNullScenarioInfo()
    {
        executionInfo.CurrentScenario = null;
        var mockActivatorWrapper = new Mock<IActivatorWrapper>();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.Scenario))).Verifiable();
        new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
        mockActivatorWrapper.VerifyAll();
    }

    [Test]
    public void ShouldMapStepDetails()
    {
        var mockActivatorWrapper = new Mock<IActivatorWrapper>();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.StepDetails),
            executionInfo.CurrentStep.Step.ActualStepText, executionInfo.CurrentStep.IsFailed,
            executionInfo.CurrentStep.StackTrace, executionInfo.CurrentStep.ErrorMessage, new List<List<string>>())).Verifiable();
        new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
        mockActivatorWrapper.VerifyAll();
    }

    [Test]
    public void ShouldMapNullStepDetails()
    {
        executionInfo.CurrentStep = null;
        var mockActivatorWrapper = new Mock<IActivatorWrapper>();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.StepDetails))).Verifiable();
        new ExecutionInfoMapper(mockAssemblyLoader.Object, mockActivatorWrapper.Object).ExecutionContextFrom(executionInfo);
        mockActivatorWrapper.VerifyAll();
    }
}