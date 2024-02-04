using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;

namespace Protocols.External._M__MinigameHandler;

public class FinishCutscene : ExternalProtocol
{
    public override string ProtocolName => "Mf";

    public override void Run(string[] message)
    {
        var arenaObjectId = message[5];

        TriggerArenaComp arena = null;

        if (Player.Room.Entities.TryGetValue(arenaObjectId, out var foundEntity))
            foreach (var component in foundEntity)
                if (component is TriggerArenaComp arenaComponent)
                    arena = arenaComponent;

        if (arena == null)
            return;

        var players = new SeparatedStringBuilder('%');

        foreach (var playerId in arena.GetPhysicalInteractorIds())
            players.Append(playerId);

        Player.SendXt("Mc", arenaObjectId, Player.Room.Time, 5f, players.ToString());
    }
}
