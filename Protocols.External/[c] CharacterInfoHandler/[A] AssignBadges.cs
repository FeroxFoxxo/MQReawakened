using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;

namespace Protocols.External._c__CharacterInfoHandler;

public class AssignBadges : ExternalProtocol
{
    public override string ProtocolName => "cA";

    public override void Run(string[] message)
    {
        var tribeData = new List<TribeDataModel>();

        foreach (var tribe in message.Skip(5))
        {
            if (string.IsNullOrEmpty(tribe))
                continue;

            tribeData.Add(new TribeDataModel(tribe));
        }

        var autoAssign = Player.Character.TribesProgression.Count % 5 == 0;

        Player.Character.Write.TribesProgression = tribeData.ToDictionary(x => x.TribeType, x => x);

        SendXt("cA", GenerateTribeData(tribeData), (int)Player.Character.Allegiance, autoAssign ? 1 : 0);
    }

    private static string GenerateTribeData(List<TribeDataModel> dataList)
    {
        var sb = new SeparatedStringBuilder('<');

        foreach (var tribeData in dataList)
            sb.Append(tribeData.ToString());

        return sb.ToString();
    }
}
