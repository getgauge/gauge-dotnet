using Gauge.Dotnet.DataStore;
using Gauge.Dotnet.Executors;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Processors;
using Gauge.Dotnet.Wrappers;
using Gauge.Messages;
using static Gauge.Dotnet.Constants;

namespace Gauge.Dotnet.UnitTests;

[TestFixture]
public class ExecutionInfoMapperTests
{
    private ExecutionInfo executionInfo;
    private Mock<IAssemblyLoader> _mockAssemblyLoader;
    private readonly Mock<IDataStoreFactory> _mockDataStoreFactory = new();
    private readonly Mock<ITableFormatter> _mockTableFormatter = new();

    [SetUp]
    public void Setup()
    {
        _mockAssemblyLoader = new Mock<IAssemblyLoader>();
        _mockAssemblyLoader.Setup(x => x.GetLibType(LibType.ExecutionContext)).Returns(typeof(CSharp.Lib.ExecutionContext));
        _mockDataStoreFactory.Setup(x => x.GetDataStoresByStream(1)).Returns(new Dictionary<DataStoreType, object>());
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
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.CurrentDataStores)))
            .Returns(new CSharp.Lib.ExecutionContext.CurrentDataStores());
        new ExecutionInfoMapper(_mockAssemblyLoader.Object, mockActivatorWrapper.Object, _mockDataStoreFactory.Object, _mockTableFormatter.Object)
            .ExecutionContextFrom(executionInfo, 1);
        mockActivatorWrapper.VerifyAll();
    }

    [Test]
    public void ShouldMapNullSpecInfo()
    {
        executionInfo.CurrentSpec = null;
        var mockActivatorWrapper = new Mock<IActivatorWrapper>();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.Specification))).Verifiable();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.CurrentDataStores)))
            .Returns(new CSharp.Lib.ExecutionContext.CurrentDataStores());
        new ExecutionInfoMapper(_mockAssemblyLoader.Object, mockActivatorWrapper.Object, _mockDataStoreFactory.Object, _mockTableFormatter.Object)
            .ExecutionContextFrom(executionInfo, 1);
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
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.CurrentDataStores)))
            .Returns(new CSharp.Lib.ExecutionContext.CurrentDataStores());
        new ExecutionInfoMapper(_mockAssemblyLoader.Object, mockActivatorWrapper.Object, _mockDataStoreFactory.Object, _mockTableFormatter.Object)
            .ExecutionContextFrom(executionInfo, 1);
        mockActivatorWrapper.VerifyAll();
    }

    [Test]
    public void ShouldMapNullScenarioInfo()
    {
        executionInfo.CurrentScenario = null;
        var mockActivatorWrapper = new Mock<IActivatorWrapper>();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.Scenario))).Verifiable();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.CurrentDataStores)))
            .Returns(new CSharp.Lib.ExecutionContext.CurrentDataStores());
        new ExecutionInfoMapper(_mockAssemblyLoader.Object, mockActivatorWrapper.Object, _mockDataStoreFactory.Object, _mockTableFormatter.Object)
            .ExecutionContextFrom(executionInfo, 1);
        mockActivatorWrapper.VerifyAll();
    }

    [Test]
    public void ShouldMapStepDetails()
    {
        var mockActivatorWrapper = new Mock<IActivatorWrapper>();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.StepDetails),
            executionInfo.CurrentStep.Step.ActualStepText, executionInfo.CurrentStep.IsFailed,
            executionInfo.CurrentStep.StackTrace, executionInfo.CurrentStep.ErrorMessage, new List<List<string>>())).Verifiable();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.CurrentDataStores)))
            .Returns(new CSharp.Lib.ExecutionContext.CurrentDataStores());
        new ExecutionInfoMapper(_mockAssemblyLoader.Object, mockActivatorWrapper.Object, _mockDataStoreFactory.Object, _mockTableFormatter.Object)
            .ExecutionContextFrom(executionInfo, 1);
        mockActivatorWrapper.VerifyAll();
    }

    [Test]
    public void ShouldMapNullStepDetails()
    {
        executionInfo.CurrentStep = null;
        var mockActivatorWrapper = new Mock<IActivatorWrapper>();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.StepDetails))).Verifiable();
        mockActivatorWrapper.Setup(x => x.CreateInstance(typeof(CSharp.Lib.ExecutionContext.CurrentDataStores)))
            .Returns(new CSharp.Lib.ExecutionContext.CurrentDataStores());
        new ExecutionInfoMapper(_mockAssemblyLoader.Object, mockActivatorWrapper.Object, _mockDataStoreFactory.Object, _mockTableFormatter.Object)
            .ExecutionContextFrom(executionInfo, 1);
        mockActivatorWrapper.VerifyAll();
    }
}