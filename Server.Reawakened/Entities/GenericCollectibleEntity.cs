using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class GenericCollectibleModel : SyncedEntity<GenericCollectible>
{
    public ILogger<GenericCollectibleModel> Logger { get; set; }

    public bool Collected;
    public int Value;

    public override void InitializeEntity()
    {
        Collected = false;

        switch (PrefabName)
        {
            case "BananaGrapCollectible":
                Value = 5;
                break;
            case "BananeCollectible":
                Value = 1;
                break;
            default:
                Logger.LogInformation("Collectible not implemented for {PrefabName}", PrefabName);
                break;
        }
    }

    public override object[] GetInitData(NetState netState) =>
        Collected ? new object[] { 0 } : Array.Empty<object>();

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        Collected = true;
        var collectedValue = Value * Level.Clients.Count;

        var currentPlayer = netState.Get<Player>();

        currentPlayer.SentEntityTriggered(Id, Level);

        var effectName = string.Empty;

        switch (PrefabName)
        {
            case "BananaGrapCollectible":
                effectName = "PF_FX_Banana_Level_01";
                break;
            case "BananeCollectible":
                effectName = "PF_FX_Banana_Level_02";
                break;
            default:
                Logger.LogInformation("Collectible not implemented for {PrefabName}", PrefabName);
                break;
        }

        var effectEvent = new FX_SyncEvent(Id.ToString(), Level.Time, effectName,
            Position.X, Position.Y, FX_SyncEvent.FXState.Play);

        Level.SendSyncEvent(effectEvent);

        if (collectedValue <= 0)
            return;

        foreach (var client in Level.Clients.Values)
            client.Get<Player>().AddBananas(client, collectedValue);
    }
}
