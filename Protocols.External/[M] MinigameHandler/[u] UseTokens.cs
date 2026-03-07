using Server.Reawakened.Network.Protocols;
using static A2m.Server.MinigameHandler;

namespace Protocols.External._M__MinigameHandler;
public class UseTokens : ExternalProtocol
{
    public override string ProtocolName => "Mu";

    public override void Run(string[] message)
    {
        var gameName = message[5];

        Player.Character.Write.Tokens -= 1;

        SendXt("Mg", 0, gameName, Player.Character.Tokens, DateTime.Now.AddYears(10).ToString(), 1, (int)AvailablePlaysSource.USE);
    }
}
