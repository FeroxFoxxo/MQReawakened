namespace Server.Colliders.Abstractions;

public interface IColliderSubscriptionTracker
{
    void Subscribe(string connectionId, int levelId, int roomInstanceId);
    void Unsubscribe(string connectionId, int levelId, int roomInstanceId);
    void RemoveAll(string connectionId);
    bool HasAnySubscribers();
    bool HasSubscribers(int levelId, int roomInstanceId);
    int TotalSubscribers { get; }
}
