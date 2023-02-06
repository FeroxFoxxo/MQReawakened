namespace Server.Reawakened.Levels.SyncedData.Abstractions;

public class EntityType
{
    public readonly Dictionary<int, BaseSynchronizedEntity> Entities;

    public EntityType() => Entities = new Dictionary<int, BaseSynchronizedEntity>();
}
