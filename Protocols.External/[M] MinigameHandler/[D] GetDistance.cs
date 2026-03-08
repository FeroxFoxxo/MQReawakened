using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._M__MinigameHandler;
public class GetDistance : ExternalProtocol
{
    public override string ProtocolName => "MD";

    public override void Run(string[] message)
    {
        if (Player.Character.BestMinigameTimes.TryGetValue(Player.Room.LevelInfo.Name, out var value))
            Player.SendXt("MD", (long)value);
    }
}
