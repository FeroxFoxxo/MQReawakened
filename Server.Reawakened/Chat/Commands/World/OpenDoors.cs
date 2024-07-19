using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.GameObjects.MonkeyGadgets;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.World;
public class OpenDoors : SlashCommand
{
    public override string CommandName => "/OpenDoors";

    public override string CommandDescription => "Opens all doors in your current room.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public ServerRConfig ServerRConfig { get; set; }

    public override void Execute(Player player, string[] args)
    {
        foreach (var triggerEntity in player.Room.GetEntitiesFromType<TriggerReceiverComp>())
        {
            if (ServerRConfig.IgnoredDoors.Contains(triggerEntity.PrefabName))
                continue;

            triggerEntity.Trigger(true, player.GameObjectId);
        }

        foreach (var vineEntity in player.Room.GetEntitiesFromType<MysticCharmTargetComp>())
            vineEntity.Charm(player);
    }
}
