using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._M__MinigameHandler;

public class FinishCutscene : ExternalProtocol
{
    public override string ProtocolName => "Mf";

    public override void Run(string[] message)
    {
        var arenaObjectId = message[5];

        Player.SendXt("Mc", arenaObjectId, Player.Room.Time, 5f,

        Player.TempData.ArenaModel.FirstPlayerId, Player.TempData.ArenaModel.SecondPlayerId,
            Player.TempData.ArenaModel.ThirdPlayerId, Player.TempData.ArenaModel.FourthPlayerId);
    }
}
