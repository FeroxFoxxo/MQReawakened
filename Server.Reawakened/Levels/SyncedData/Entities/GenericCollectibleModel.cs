using Server.Base.Network;
using Server.Reawakened.Levels.SyncedData.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using SmartFoxClientAPI.Data;
using UnityEngine;

namespace Server.Reawakened.Levels.SyncedData.Entities;

public class GenericCollectibleModel : SynchronizedEntity<GenericCollectible>
{
    public readonly int Value;

    public bool Collected;

    public GenericCollectibleModel(StoredEntityModel storedEntity,
        GenericCollectible entityData) : base(storedEntity, entityData)
    {
        Collected = false;

        Value = storedEntity.PrefabName switch
        {
            "BananaGrapCollectible" => 5,
            "BananeCollectible" => 1,
            _ => throw new InvalidDataException(storedEntity.PrefabName)
        };
    }

    public override string[] GetInitData() => new string[Collected ? 1 : 0];

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        Collected = true;
        var collectedValue = Value * StoredEntity.Level.Clients.Count;

        var currentPlayer = netState.Get<Player>();

        var collectedEvent = new Trigger_SyncEvent(StoredEntity.Id.ToString(), StoredEntity.Level.Time, true,
            currentPlayer.PlayerId.ToString(), true);

        StoredEntity.Level.SendSyncEvent(collectedEvent);
        
        var effectName = StoredEntity.PrefabName switch
        {
            "BananaGrapCollectible" => "PF_FX_Banana_Level_01",
            "BananeCollectible" => "PF_FX_Banana_Level_02",
            _ => throw new InvalidDataException(StoredEntity.PrefabName)
        };

        var effectEvent = new FX_SyncEvent(StoredEntity.Id.ToString(), StoredEntity.Level.Time, effectName,
            StoredEntity.Position.X, StoredEntity.Position.Y, FX_SyncEvent.FXState.Play);

        StoredEntity.Level.SendSyncEvent(effectEvent);

        foreach (var client in StoredEntity.Level.Clients.Values)
            client.AddBananas(collectedValue);
    }
}
