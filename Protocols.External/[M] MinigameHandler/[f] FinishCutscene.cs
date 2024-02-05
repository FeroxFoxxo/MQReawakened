using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._M__MinigameHandler;

public class FinishCutscene : ExternalProtocol
{
    public override string ProtocolName => "Mf";
    public ILogger<FinishCutscene> Logger { get; set; }

    public override void Run(string[] message)
    {
        var arenaObjectId = message[5];

        TriggerCoopControllerComp trigger = null;

        if (Player.Room.Entities.TryGetValue(arenaObjectId, out var foundEntity))
            foreach (var component in foundEntity)
                if (component is TriggerCoopControllerComp foundTrigger)
                    trigger = foundTrigger;

        if (trigger == null)
        {
            Logger.LogError("Could not find cutscene finish trigger for {Id}", arenaObjectId);
            return;
        }

        var players = new SeparatedStringBuilder('%');

        foreach (var playerId in trigger.GetPhysicalInteractorIds())
            players.Append(playerId);

        Player.SendXt("Mc", arenaObjectId, Player.Room.Time, 5f, players.ToString());
    }
}
