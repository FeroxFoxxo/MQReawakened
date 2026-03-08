using Server.Reawakened.Network.Protocols;
using static A2m.Server.MinigameHandler;

namespace Protocols.External._M__MinigameHandler;
public class GetAvailableTokens : ExternalProtocol
{
    public override string ProtocolName => "Mg";

    public override void Run(string[] message)
    {
        var gameName = message[5];

        SendXt("Mg", 0, gameName, Player.Character.Write.Tokens, DateTime.Now.AddYears(10).ToString(), 1, (int)AvailablePlaysSource.GET);
    }
}
