
namespace Gauge.Dotnet.DataStore;

public interface IDataStoreFactory
{
    dynamic SuiteDataStore { get; }

    void AddDataStore(int stream, DataStoreType storeType);
    IReadOnlyDictionary<DataStoreType, dynamic> GetDataStoresByStream(int streamId);
}