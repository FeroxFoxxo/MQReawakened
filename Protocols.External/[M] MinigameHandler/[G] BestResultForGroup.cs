using Newtonsoft.Json.Linq;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._M__MinigameHandler;

public class BestResultForGroup : ExternalProtocol
{
    public override string ProtocolName => "MG";
    public WorldGraph WorldGraph { get; set; }

    public override void Run(string[] message)
    {
        var sb = new SeparatedStringBuilder('|');
        var level = WorldGraph.LevelNameFromID(Player.Room.LevelInfo.LevelId);

        foreach (var gamer in Player.Room.GetPlayers())
            if (gamer.Character.BestMinigameTimes.TryGetValue(level, out var time))
            {
                sb.Append(gamer.GameObjectId);
                sb.Append(time * 1000);
            }

        Player.SendXt("MG", sb.ToString());
    }
}
