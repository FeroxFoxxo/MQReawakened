using System.Collections.Concurrent;

namespace Server.Colliders.Services;

public class InMemoryColliderSubscriptionTracker
{
    private readonly ConcurrentDictionary<(int,int), ConcurrentDictionary<string, byte>> _roomSubs = new();
    private readonly ConcurrentDictionary<string, HashSet<(int,int)>> _connIndex = new();

    public void Subscribe(string connectionId, int levelId, int roomInstanceId)
    {
        var key = (levelId, roomInstanceId);
        var set = _roomSubs.GetOrAdd(key, _ => new ConcurrentDictionary<string, byte>());
        set[connectionId] = 1;
        _connIndex.AddOrUpdate(connectionId,
            _ => [key],
            (_, existing) => { lock (existing) { existing.Add(key); } return existing; }
        );
    }

    public void Unsubscribe(string connectionId, int levelId, int roomInstanceId)
    {
        var key = (levelId, roomInstanceId);
        if (_roomSubs.TryGetValue(key, out var set))
        {
            set.TryRemove(connectionId, out _);
            if (set.IsEmpty) _roomSubs.TryRemove(key, out _);
        }
        if (_connIndex.TryGetValue(connectionId, out var rooms))
        {
            lock(rooms) rooms.Remove(key);
            if (rooms.Count == 0) _connIndex.TryRemove(connectionId, out _);
        }
    }

    public IReadOnlyCollection<(int levelId,int roomInstanceId)> RemoveAll(string connectionId)
    {
        var removed = new List<(int,int)>();
        if (_connIndex.TryRemove(connectionId, out var rooms))
        {
            lock(rooms)
            {
                foreach (var key in rooms)
                {
                    if (_roomSubs.TryGetValue(key, out var set))
                    {
                        set.TryRemove(connectionId, out _);
                        if (set.IsEmpty) _roomSubs.TryRemove(key, out _);
                    }
                    removed.Add(key);
                }
            }
        }
        return removed;
    }

    public bool HasAnySubscribers() => _roomSubs.Any(static kvp => !kvp.Value.IsEmpty);

    public bool HasSubscribers(int levelId, int roomInstanceId) => _roomSubs.TryGetValue((levelId, roomInstanceId), out var set) && !set.IsEmpty;

    public int GetRoomSubscriberCount(int levelId, int roomInstanceId)
        => _roomSubs.TryGetValue((levelId, roomInstanceId), out var set) ? set.Count : 0;
}
