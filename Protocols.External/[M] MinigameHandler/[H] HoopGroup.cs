using Server.Reawakened.Network.Protocols;

namespace Protocols.External._M__MinigameHandler;

public class HoopGroup : ExternalProtocol
{
    public override string ProtocolName => "MH";

    public override void Run(string[] message)
    {
        var completed = int.Parse(message[5]) == 1;
        var numberOfHoops = int.Parse(message[6]);
        var hoopGroupName = string.Empty;

        if (message.Length > 7)
            if (!string.IsNullOrEmpty(message[7]))
                hoopGroupName = message[7];

        Console.WriteLine("HIT HOOP");
    }
}
