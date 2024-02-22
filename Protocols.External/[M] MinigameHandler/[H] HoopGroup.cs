using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;

namespace Protocols.External._M__MinigameHandler;

public class HoopGroup : ExternalProtocol
{
    public override string ProtocolName => "MH";

    public ILogger<HoopGroup> Logger { get; set; }

    public QuestCatalog QuestCatalog { get; set; }
    public InternalAchievement InternalAchievement { get; set; }

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
            var hoops = Player.Room.GetEntitiesFromType<HoopControllerComp>();

            var masterHoop = hoops.First(x => x.HoopGroupStringId == hoopGroupName && x.IsMasterController);

            var hitHoops = hoops.Where(x => x.HoopGroupId == masterHoop.HoopGroupId);

            foreach (var hoop in hitHoops)
            {
                Player.CheckObjective(ObjectiveEnum.Invalid, hoop.Id, hoop.PrefabName, 1, QuestCatalog);

                Player.CheckAchievement(AchConditionType.Hoop, string.Empty, InternalAchievement, Logger);
                Player.CheckAchievement(AchConditionType.HoopInLevel, Player.Room.LevelInfo.Name, InternalAchievement, Logger);
            }

            Player.CheckAchievement(AchConditionType.HoopGroup, string.Empty, InternalAchievement, Logger);
            Player.CheckAchievement(AchConditionType.HoopGroupInLevel, Player.Room.LevelInfo.Name, InternalAchievement, Logger);
        }
    }
}
