using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Levels.Extensions;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models.Character;
using WorldGraphDefines;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlayLevel : ExternalProtocol
{
    public LevelHandler LevelHandler { get; set; }

    public override string ProtocolName => "lz";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var level = LevelHandler.GetLevelFromId(player.UserInfo.CharacterLevel[player.CurrentCharacter]);
        player.JoinLevel(NetState, level, out var reason);

        SendXt("lz", reason.GetJoinReasonError(), level.LevelData.LevelId, level.LevelData.Name);

        if (TribeDiscovered(level.LevelData, player.GetCurrentCharacter()))
            SendXt("cB", (int) level.LevelData.Tribe);
    }

    public static bool TribeDiscovered(LevelInfo lInfo, CharacterDataModel character)
    {
        if (!lInfo.Name.Contains("highway", StringComparison.OrdinalIgnoreCase))
            return false;

        if (character.TribesDiscovered.ContainsKey(lInfo.Tribe))
        {
            if (character.TribesDiscovered[lInfo.Tribe])
                return false;

            character.TribesDiscovered[lInfo.Tribe] = true;
        }
        else
        {
            character.TribesDiscovered.Add(lInfo.Tribe, true);
        }

        return true;
    }
}
