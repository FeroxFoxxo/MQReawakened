using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models.Minigames;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._M__MinigameHandler;

public class FinishCutscene : ExternalProtocol
{
    public override string ProtocolName => "Mf";

    public LootCatalogInt LootCatalog { get; set; } 
    public ArenaCatalogInt ArenaCatalogInt { get; set; }
    public override async void Run(string[] message)
    {
        var arenaObjectId = int.Parse(message[5]);

        Player.SendXt("Mc", arenaObjectId, Player.Room.Time, 5f,
        ArenaModel.FirstPlayerId, ArenaModel.SecondPlayerId,
            ArenaModel.ThirdPlayerId, ArenaModel.FourthPlayerId);

        ArenaModel.LootCatalog = LootCatalog;

        await Task.Delay(ArenaCatalogInt.Arena[arenaObjectId].MinigameTimeLimit);

        var playersInRace = ArenaModel.Participants;

        foreach (var player in playersInRace)
        {
            if (ArenaModel.ArenaActivated || player.TempData.ArenaModel.ActivatedSwitch)
            {
                player.TempData.ArenaModel.ActivatedSwitch = false;
                player.SendXt("Ml", arenaObjectId, Player.Room.Time);
            }
        }
        await Task.Delay(5000);

        if (playersInRace.All(p => !p.TempData.ArenaModel.ActivatedSwitch))
            foreach (var player in playersInRace)
                ArenaModel.FinishMinigame(arenaObjectId, player);
    }
}
