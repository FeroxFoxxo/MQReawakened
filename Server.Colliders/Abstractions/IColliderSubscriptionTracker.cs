namespace Server.Colliders.Abstractions;

public interface IColliderSubscriptionTracker
{
    void Subscribe(string connectionId, int levelId, int roomInstanceId);
    void Unsubscribe(string connectionId, int levelId, int roomInstanceId);
    /// <summary>Remove all subscriptions for a connection and return affected rooms.</summary>
    IReadOnlyCollection<(int levelId,int roomInstanceId)> RemoveAll(string connectionId);
    bool HasAnySubscribers();
    bool HasSubscribers(int levelId, int roomInstanceId);
    int GetRoomSubscriberCount(int levelId, int roomInstanceId);
    int TotalSubscribers { get; }
}
