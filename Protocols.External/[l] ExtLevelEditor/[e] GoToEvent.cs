using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._l__ExtLevelEditor;

public class GoToEvent : ExternalProtocol
{
    public override string ProtocolName => "le";

    public ILogger<GoToEvent> Logger { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var destinationIds = message[5].Split('|');

        var levelId = int.Parse(destinationIds[0]);
        var spawnId = int.Parse(destinationIds[1]);

        character.SetLevel(levelId, spawnId, Logger);

        Player.SendLevelChange();
    }
}
