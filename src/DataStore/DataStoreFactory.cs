using System.Collections.Concurrent;
using Gauge.Dotnet.Loaders;
using Gauge.Dotnet.Wrappers;

namespace Gauge.Dotnet.DataStore;

public class DataStoreFactory(IAssemblyLoader assemblyLoader, IActivatorWrapper activator) : IDataStoreFactory
{
    private readonly Type _dataStoreType = assemblyLoader.GetLibType(LibType.DataStore);
    private object _suiteDataStore = null;
    private readonly object _suiteDataStoreLock = new();
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<DataStoreType, dynamic>> _dataStores = new();

    public dynamic SuiteDataStore
    {
        get
        {
            lock (_suiteDataStoreLock)
            {
                return _suiteDataStore;
            }
        }
        private set
        {
            lock (_suiteDataStoreLock)
            {
                _suiteDataStore ??= value;
            }
        }
    }

    public IReadOnlyDictionary<DataStoreType, dynamic> GetDataStoresByStream(int streamId)
    {
        return _dataStores.GetValueOrDefault(streamId, new());
    }

    public void AddDataStore(int stream, DataStoreType storeType)
    {
        dynamic dataStore = activator.CreateInstance(_dataStoreType);
        switch (storeType)
        {
            case DataStoreType.Suite: SuiteDataStore = dataStore; break;
            case DataStoreType.Spec:
            case DataStoreType.Scenario: SetStreamDataStore(stream, storeType, dataStore); break;
        }
    }

    private void SetStreamDataStore(int stream, DataStoreType storeType, dynamic dataStore)
    {
        if (!_dataStores.TryGetValue(stream, out ConcurrentDictionary<DataStoreType, dynamic> value))
        {
            value = new ConcurrentDictionary<DataStoreType, object>();
            _dataStores[stream] = value;
        }

        value[storeType] = dataStore;
    }
}
