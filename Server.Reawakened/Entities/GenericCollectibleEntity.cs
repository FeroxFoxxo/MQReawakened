using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class GenericCollectibleModel : SyncedEntity<GenericCollectible>
{
    public int Value;
    public bool Collected;

    public override void InitializeEntity()
    {
        Collected = false;

        Value = PrefabName switch
        {
            "BananaGrapCollectible" => 5,
            "BananeCollectible" => 1,
            _ => throw new InvalidDataException(PrefabName)
        };
    }

    public override string[] GetInitData(NetState netState) => new[] { Collected ? "0" : "1" };

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        Collected = true;
        var collectedValue = Value * Level.Clients.Count;

        var currentPlayer = netState.Get<Player>();

        currentPlayer.SentEntityTriggered(Id, Level);

        var effectName = PrefabName switch
        {
            "BananaGrapCollectible" => "PF_FX_Banana_Level_01",
            "BananeCollectible" => "PF_FX_Banana_Level_02",
            _ => throw new InvalidDataException(PrefabName)
        };

        var effectEvent = new FX_SyncEvent(Id.ToString(), Level.Time, effectName,
            Position.X, Position.Y, FX_SyncEvent.FXState.Play);

        Level.SendSyncEvent(effectEvent);

        foreach (var client in Level.Clients.Values)
            client.Get<Player>().AddBananas(client, collectedValue);
    }
}
