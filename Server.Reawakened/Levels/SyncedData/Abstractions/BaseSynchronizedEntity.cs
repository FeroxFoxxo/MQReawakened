using Microsoft.Extensions.Logging;
using Server.Base.Network;

namespace Server.Reawakened.Levels.SyncedData.Abstractions;

public abstract class BaseSynchronizedEntity
{
    public readonly StoredEntityModel StoredEntity;

    protected BaseSynchronizedEntity(StoredEntityModel storedEntity) =>
        StoredEntity = storedEntity;

    public virtual string InitData(NetState netState) =>
        string.Empty;

    public virtual void SendEntityInformationToClient(string[] message, NetState netState) =>
        StoredEntity.Logger.LogError(
            "Tried sending entity information of {Id}, which has no override! Skipping...",
            StoredEntity.Id
        );
}
