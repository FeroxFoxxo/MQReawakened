using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._l__ExtLevelEditor;

public class GoToEvent : ExternalProtocol
{
    public override string ProtocolName => "le";

    public ILogger<GoToEvent> Logger { get; set; }
    public EventPrefabs EventPrefabs { get; set; }
    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        var destinationIds = message[5].Split('|');
        var levelId = int.Parse(destinationIds[0]);
        var spawnId = 0;

        if (destinationIds.Length > 1)
        {
            spawnId = int.Parse(destinationIds[1]);
        }
        else
        {
            if (EventPrefabs.EventIdToLevelId.TryGetValue(levelId, out var level))
            {
                if (!int.TryParse(level, out levelId))
                    return;
            }
            else return;
        }

        WorldHandler.ChangePlayerRoom(Player, levelId, spawnId.ToString());
    }
}
