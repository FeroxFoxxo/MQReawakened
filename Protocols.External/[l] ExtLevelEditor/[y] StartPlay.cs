using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles;
using WorldGraphDefines;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlay : ExternalProtocol
{
    public LevelHandler LevelHandler { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override string ProtocolName => "ly";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var error = string.Empty;
        var levelName = string.Empty;
        var surroundingLevels = string.Empty;

        try
        {
            var level = player.GetCurrentLevel(LevelHandler);

            levelName = level.LevelInfo.Name;
            surroundingLevels = GetSurroundingLevels(level.LevelInfo);
        }
        catch (Exception e)
        {
            error = e.Message;
        }

        NetState.SendXt("lw", error, levelName, surroundingLevels);
    }

    public string GetSurroundingLevels(LevelInfo levelInfo)
    {
        var sb = new SeparatedStringBuilder('!');

        var levels = WorldGraph.GetLevelWorldGraphNodes(levelInfo.LevelId)
            .Where(x => x.ToLevelID != x.LevelID)
            .Select(x => WorldGraph.GetInfoLevel(x.ToLevelID).Name)
            .Distinct();

        foreach (var level in levels)
            sb.Append(level);

        return sb.ToString();
    }
}
