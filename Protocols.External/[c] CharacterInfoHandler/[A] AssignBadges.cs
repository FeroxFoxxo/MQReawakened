using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;

namespace Protocols.External._c__CharacterInfoHandler;

public class AssignBadges : ExternalProtocol
{
    public override string ProtocolName => "cA";

    public override void Run(string[] message)
    {
        foreach (var tribe in message.Skip(5))
        {
            if (string.IsNullOrEmpty(tribe))
                continue;

            var tribeData = new TribeDataModel(tribe);

            if (Player.Character.Write.TribesProgression.TryGetValue(tribeData.TribeType, out var value))
                value.BadgePoints = tribeData.BadgePoints;
            else
                Player.Character.Write.TribesProgression.TryAdd(tribeData.TribeType, tribeData);
        }

        var autoAssign = Player.Character.TribesProgression.Count % 5 == 0;

        SendXt("cA", Player.Character.GenerateTribeData([.. Player.Character.TribesProgression.Values]), (int)Player.Character.Allegiance, autoAssign ? 1 : 0);
    }
}
