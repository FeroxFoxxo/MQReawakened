using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._c__CharacterInfoHandler;

public class EventCompleted : ExternalProtocol
{
    public override string ProtocolName => "cE";

    public override void Run(string[] message)
    {
        var events = message[5]
            .Split("|")
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(int.Parse)
            .ToArray();

        foreach (var e in events)
            if (!Player.TempData.Events.Contains(e))
                Player.TempData.Events.Add(e);

        var eventList = EventList(Player.TempData.Events);

        Player.SendXt("cE", eventList);
    }

    private static string EventList(List<int> events)
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var e in events)
            sb.Append(e);

        return sb.ToString();
    }
}
