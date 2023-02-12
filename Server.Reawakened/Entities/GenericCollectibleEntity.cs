using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class GenericCollectibleModel : SyncedEntity<GenericCollectible>
{
    public bool Collected;
    public int Value;

    public override void InitializeEntity()
    {
        Collected = false;

        Value = PrefabName switch
        {
            "BananaGrapCollectible" => 5,
            "BananeCollectible" => 1,
            "PF_SHD_SwingingVine01" => 0,
            _ => throw new InvalidDataException(PrefabName)
        };
    }

    public override string[] GetInitData(NetState netState) =>
        Collected ? new[] { "0" } : Array.Empty<string>();

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
            "PF_SHD_SwingingVine01" => "PF_FX_Vine_Climb",
            _ => throw new InvalidDataException(PrefabName)
        };

        var effectEvent = new FX_SyncEvent(Id.ToString(), Level.Time, effectName,
            Position.X, Position.Y, FX_SyncEvent.FXState.Play);

        Level.SendSyncEvent(effectEvent);

        if (collectedValue <= 0)
            return;

        foreach (var client in Level.Clients.Values)
            client.Get<Player>().AddBananas(client, collectedValue);
    }
}
