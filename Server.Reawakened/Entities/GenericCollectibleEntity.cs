using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class GenericCollectibleModel : SynchronizedEntity<GenericCollectible>
{
    public int Value;
    public bool Collected;

    public override void InitializeEntity()
    {
        Collected = false;

        Value = StoredEntity.PrefabName switch
        {
            "BananaGrapCollectible" => 5,
            "BananeCollectible" => 1,
            _ => throw new InvalidDataException(StoredEntity.PrefabName)
        };
    }

    public override string[] GetInitData() => new string[Collected ? 1 : 0];

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        Collected = true;
        var collectedValue = Value * Level.Clients.Count;

        var currentPlayer = netState.Get<Player>();
        var currentCharacter = currentPlayer.GetCurrentCharacter();
        var characterId = currentCharacter.GetCharacterObjectId().ToString();

        var collectedEvent = new Trigger_SyncEvent(StoredEntity.Id.ToString(), Level.Time, true,
            characterId, true);

        Level.SendSyncEvent(collectedEvent);

        var effectName = StoredEntity.PrefabName switch
        {
            "BananaGrapCollectible" => "PF_FX_Banana_Level_01",
            "BananeCollectible" => "PF_FX_Banana_Level_02",
            _ => throw new InvalidDataException(StoredEntity.PrefabName)
        };

        var effectEvent = new FX_SyncEvent(StoredEntity.Id.ToString(), Level.Time, effectName,
            StoredEntity.Position.X, StoredEntity.Position.Y, FX_SyncEvent.FXState.Play);

        Level.SendSyncEvent(effectEvent);

        foreach (var client in Level.Clients.Values)
            client.Get<Player>().AddBananas(client, collectedValue);
    }
}
