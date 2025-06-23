using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Network;
using Server.Base.Network.Events;
using Server.Reawakened.Core.Configs;

namespace Server.Reawakened.Network.Services;

public class RandomKeyGenerator(Random random, EventSink sink, ServerRConfig config) : IService
{
    private readonly Dictionary<Type, Dictionary<string, string>> _keys = [];

    public void Initialize()
    {
        sink.NetStateAdded += AddedNetState;
        sink.NetStateRemoved += RemovedNetState;
    }

    private Dictionary<string, string> CheckIfExists<T>()
    {
        if (!_keys.ContainsKey(typeof(T)))
            _keys.Add(typeof(T), []);
        return _keys[typeof(T)];
    }

    private void RemovedNetState(NetStateRemovedEventArgs @event)
    {
        var id = @event.State.ToString();

        var rKeys = CheckIfExists<NetState>();

        rKeys.Remove(id);
    }

    private void AddedNetState(NetStateAddedEventArgs @event)
    {
        var id = @event.State.ToString();

        var rKeys = CheckIfExists<NetState>();

        if (!rKeys.ContainsKey(id))
            rKeys.Add(id, GetRandomKey(config.RandomKeyLength));
    }

    public string GetRandomKey<T>(string id)
    {
        var rKeys = CheckIfExists<T>();

        if (rKeys.TryGetValue(id, out var value))
            return value;

        while (true)
        {
            var rKey = GetRandomKey(config.RandomKeyLength);

            if (rKeys.ContainsValue(rKey))
                continue;

            return rKey;
        }
    }

    private string GetRandomKey(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
    }
}
