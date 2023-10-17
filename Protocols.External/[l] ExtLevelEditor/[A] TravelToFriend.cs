using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._l__ExtLevelEditor;
public class TravelToFriend : ExternalProtocol
{
    public override string ProtocolName => "lA";

    public WorldGraph WorldGraph { get; set; }

    public WorldHandler WorldHandler { get; set; }

    public PlayerHandler PlayerHandler { get; set; }

    public ILogger<TravelToFriend> Logger { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var playerName = message[5];    

        var otherPlayer = PlayerHandler.PlayerList
            .FirstOrDefault(p => p.Character.Data.CharacterName == playerName);

        var levelId = otherPlayer.Character.LevelData.LevelId;
        var portalId = otherPlayer.Character.LevelData.PortalId;
        var spawnId = otherPlayer.Character.LevelData.SpawnPointId;

        character.SetLevel(levelId, spawnId, Logger);

        Player.SendLevelChange(WorldHandler, WorldGraph);
    }
}
