namespace Server.Colliders.Abstractions;

public interface IRoomVersionTracker
{
    long Increment(int levelId, int roomInstanceId);
    long Get(int levelId, int roomInstanceId);
}
