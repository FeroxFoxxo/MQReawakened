using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Models;

namespace Server.Base.Core.Services;

public class TemporaryDataStorage : IService
{
    private readonly Dictionary<string, List<PersistantData>> _data;
    private readonly EventSink _sink;

    public TemporaryDataStorage(EventSink sink)
    {
        _sink = sink;
        _data = new Dictionary<string, List<PersistantData>>();
    }

    public void Initialize() => _sink.ServerStarted += _ => _data.Clear();

    public T GetData<T>(string id) where T : PersistantData
    {
        EnsureDataExists(id);
        return _data[id].FirstOrDefault(x => x.GetType() == typeof(T)) as T;
    }

    public void AddData(string id, PersistantData data)
    {
        EnsureDataExists(id);
        if (_data[id].All(x => x.GetType() != data.GetType()))
            _data[id].Add(data);
    }

    public void EnsureDataExists(string id)
    {
        if (!_data.ContainsKey(id))
            _data.Add(id, new List<PersistantData>());
    }
}
