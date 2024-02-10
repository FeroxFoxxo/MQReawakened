using A2m.Server;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

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

        if (completed)
        {
            var hoops = Player.Room.Entities.Values.SelectMany(x => x)
                .Where(x => x is HoopControllerComp)
                .Select(x => x as HoopControllerComp);

            var masterHoop = hoops.First(x => x.HoopGroupStringId == hoopGroupName && x.IsMasterController);

            var hitHoops = hoops.Where(x => x.HoopGroupId == masterHoop.HoopGroupId);

            foreach (var hoop in hitHoops)
                Player.CheckObjective(ObjectiveEnum.Invalid, hoop.Id, hoop.PrefabName, 1);
        }
    }
}
