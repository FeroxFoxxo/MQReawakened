using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using static A2m.Server.MinigameHandler;

namespace Protocols.External._M__MinigameHandler;
public class IncreaseTokens : ExternalProtocol
{
    public override string ProtocolName => "Mi";

    public override void Run(string[] message)
    {
        var gameName = message[5];
        var tokens = int.Parse(message[6]);

        if (Player.Character.Cash < 1000)
        {
            Player.SendXt("Ce", -1);
            return;
        }

        Player.Character.Write.Tokens += tokens;
        Player.Character.Write.Cash -= 1000;

        SendXt("Mg", 0, gameName, Player.Character.Tokens, DateTime.Now.AddYears(10).ToString(), 1, (int)AvailablePlaysSource.INC);
    }
}
