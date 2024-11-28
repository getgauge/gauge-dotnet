using Gauge.Dotnet.DataStore;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Wrappers;
using static Gauge.Dotnet.Constants;

namespace Gauge.Dotnet.UnitTests.DataStore;

[TestFixture]
public class DataStoreFactoryTests
{
    private readonly Mock<IAssemblyLoader> _assemblyLoaderMock = new();
    private readonly Mock<IActivatorWrapper> _activatorWrapperMock = new();

    private DataStoreFactory _factory;

    [SetUp]
    public void Setup()
    {
        _assemblyLoaderMock.Setup(x => x.GetLibType(LibType.DataStore)).Returns(typeof(Gauge.CSharp.Lib.DataStore));
        _activatorWrapperMock.Setup(x => x.CreateInstance(typeof(Gauge.CSharp.Lib.DataStore))).Returns(() => new Gauge.CSharp.Lib.DataStore());
        _factory = new(_assemblyLoaderMock.Object, _activatorWrapperMock.Object);
    }

    [Test]
    public void AddDataStore_ShouldSetTheSuiteDataStore_WhenCalledForSuiteDataStoreType()
    {
        _factory.AddDataStore(1, DataStoreType.Suite);
        Assert.That(_factory.SuiteDataStore, Is.Not.Null);
    }

    [Test]
    public void AddDataStore_ShouldNotOverwriteTheSuiteDataStore_WhenCalledForSuiteDataStoreTypeMoreThanOnce()
    {
        _factory.AddDataStore(1, DataStoreType.Suite);
        var dataStore = _factory.SuiteDataStore;
        _factory.AddDataStore(1, DataStoreType.Suite);
        Assert.That(dataStore, Is.SameAs(_factory.SuiteDataStore));
    }

    [Test]
    public void AddDataStore_ShouldSetTheSpecDataStore_WhenCalledForSpecDataStoreType()
    {
        _factory.AddDataStore(1, DataStoreType.Spec);
        Assert.That(_factory.GetDataStoresByStream(1)[DataStoreType.Spec], Is.Not.Null);
    }

    [Test]
    public void AddDataStore_ShouldSetTheScenaroDataStore_WhenCalledForScenarioDataStoreType()
    {
        _factory.AddDataStore(1, DataStoreType.Scenario);
        Assert.That(_factory.GetDataStoresByStream(1)[DataStoreType.Scenario], Is.Not.Null);
    }

    [Test]
    public void AddDataStore_ShouldKeepSeparateDataStores_WhenCalledForDifferentStreams()
    {
        _factory.AddDataStore(1, DataStoreType.Scenario);
        _factory.AddDataStore(2, DataStoreType.Scenario);
        var dict1 = _factory.GetDataStoresByStream(1)[DataStoreType.Scenario] as Gauge.CSharp.Lib.DataStore;
        var dict2 = _factory.GetDataStoresByStream(2)[DataStoreType.Scenario] as Gauge.CSharp.Lib.DataStore;

        Assert.That(dict1, Is.Not.SameAs(dict2));
        dict1.Add("mykey", new object());
        Assert.That(dict2.Get("mykey"), Is.Null);
    }
}
