using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Models;

namespace Server.Base.Core.Services;

public class TemporaryDataStorage(EventSink sink) : IService
{
    private readonly Dictionary<string, List<PersistantData>> _data = [];

    public void Initialize() => sink.ServerStarted += _ => _data.Clear();

    public T GetData<T>(string id) where T : PersistantData
    {
        EnsureDataExists(id);
        return _data[id].FirstOrDefault(x => x.GetType() == typeof(T)) as T;
    }

    public void RemoveData<T>(string id, T data) where T : PersistantData
    {
        EnsureDataExists(id);
        _data[id].Remove(data);
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
            _data.Add(id, []);
    }
}
