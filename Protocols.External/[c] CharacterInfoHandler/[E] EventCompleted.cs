using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Protocols.External._c__CharacterInfoHandler;

public class EventCompleted : ExternalProtocol
{
    public override string ProtocolName => "cE";

    public InternalEventReward EventReward { get; set; }
    public InternalAchievement InternalAchievement { get; set; }

    public override void Run(string[] message)
    {
        var events = message[5]
            .Split("|")
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(int.Parse)
            .ToArray();

        foreach (var e in events)
            if (!Player.Character.Events.Contains(e))
            {
                Player.Character.Events.Add(e);
                EventReward.CheckEventReward(e, Player, InternalAchievement);
            }

        var eventList = EventList(Player.Character.Events);

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
