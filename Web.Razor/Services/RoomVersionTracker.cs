using System.Collections.Concurrent;

namespace Web.Razor.Services;

public class RoomVersionTracker
{
    private readonly ConcurrentDictionary<string, long> _versions = new();

    private static string Key(int levelId, int roomInstanceId) => $"{levelId}:{roomInstanceId}";

    public long Increment(int levelId, int roomInstanceId) => _versions.AddOrUpdate(Key(levelId, roomInstanceId), 1, (_, v) => v + 1);

    public long Get(int levelId, int roomInstanceId) => _versions.TryGetValue(Key(levelId, roomInstanceId), out var v) ? v : 0;
}
