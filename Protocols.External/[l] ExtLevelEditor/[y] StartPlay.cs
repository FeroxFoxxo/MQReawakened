using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlay : ExternalProtocol
{
    public LevelHandler LevelHandler { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override string ProtocolName => "ly";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        const char levelDelimiter = '!';
        var error = string.Empty;
        var levelName = string.Empty;
        var surroundingLevels = string.Empty;

        try
        {
            var level = LevelHandler.GetLevelFromId(player.UserInfo.CharacterLevel[player.CurrentCharacter]);

            levelName = level.LevelData.Name;
            surroundingLevels = string.Join(levelDelimiter,
                WorldGraph.GetLevelWorldGraphNodes(level.LevelData.LevelId)
                    .Where(x => x.ToLevelID != x.LevelID)
                    .Select(x => WorldGraph.GetInfoLevel(x.ToLevelID).Name)
                    .Distinct()
            );
        }
        catch (Exception e)
        {
            error = e.Message;
        }

        NetState.SendXt("lw", error, levelName, surroundingLevels);
    }
}
