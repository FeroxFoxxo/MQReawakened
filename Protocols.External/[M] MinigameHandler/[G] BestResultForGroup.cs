using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._M__MinigameHandler;
public class BestResultForGroup : ExternalProtocol
{
    public override string ProtocolName => "MG";

    public override void Run(string[] message)
    {
        if (Player.TempData.ArenaModel.BestTimeForLevel == null)
        {
            Player.TempData.ArenaModel.BestTimeForLevel = [];
            Player.TempData.ArenaModel.BestTimeForLevel.Add(Player.Room.LevelInfo.Name, 0f);
        }

        Player.SendXt("MG", FormatBestTime(Player.TempData.ArenaModel.BestTimeForLevel));
    }

    public string FormatBestTime(Dictionary<string, float> bestTimeForGroup)
    {
        var sb = new SeparatedStringBuilder('|'); ;

        sb.Append(Player.CharacterName);
        sb.Append(bestTimeForGroup[Player.Room.LevelInfo.Name]);

        return sb.ToString();
    }
}
