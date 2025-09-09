using System.Collections.Concurrent;
using Server.Base.Core.Abstractions;

namespace Server.Colliders.Services;

public class InMemoryRoomVersionTracker : IService
{
    private readonly ConcurrentDictionary<string,long> _versions = new();
    private static string Key(int levelId,int room) => $"{levelId}:{room}";
    public long Increment(int levelId, int roomInstanceId) => _versions.AddOrUpdate(Key(levelId,roomInstanceId),1,(_,v)=>v+1);
    public void Initialize() { }
}
