using Server.Base.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._c__CharacterInfoHandler;

public class DiscoverStat : ExternalProtocol
{
    public override string ProtocolName => "cf";

    public FileLogger FileLogger { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        var stat = int.Parse(message[5]);

        if (!character.Data.DiscoveredStats.Contains(stat))
            character.Data.DiscoveredStats.Add(stat);

        FileLogger.WriteGenericLog<DiscoverStat>("discovered-stats", "Discovered Character Stat",
            stat.ToString(), LoggerType.Trace);
    }
}
