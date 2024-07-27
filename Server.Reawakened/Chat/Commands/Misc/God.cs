using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class God : SlashCommand
{
    public override string CommandName => "/God";

    public override string CommandDescription => "Enable's god mode.";

    public override List<ParameterModel> Parameters => [];
    public override AccessLevel AccessLevel => AccessLevel.Player;

    public ItemRConfig ItemRConfig { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<God> Logger { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public WorldStatistics WorldStatistics { get; set; }

    public override void Execute(Player player, string[] args)
    {
        AddKit(player, 1);
        player.AddSlots(true, ItemRConfig);

        player.AddBananas(ServerRConfig.CashKitAmount, InternalAchievement, Logger);
        player.AddNCash(ServerRConfig.CashKitAmount);
        player.SendCashUpdate();

        player.LevelUp(ServerRConfig.MaxLevel, WorldStatistics, ServerRConfig, Logger);
        player.AddPoints();
        player.DiscoverAllTribes();

        player.Character.Write.CurrentLife = player.Character.MaxLife;

        var health = new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time, player.Character.MaxLife, player.Character.MaxLife, "now");
        player.Room.SendSyncEvent(health);

        var heal = new StatusEffect_SyncEvent(player.GameObjectId.ToString(), player.Room.Time, (int)ItemEffectType.Healing, ItemRConfig.HealAmount, 1, true, player.GameObjectId.ToString(), true);
        player.Room.SendSyncEvent(heal);
    }

    private void AddKit(Player player, int amount)
    {
        var items = ItemRConfig.SingleItemKit
            .Select(ItemCatalog.GetItemFromId)
            .ToList();

        foreach (var itemId in ItemRConfig.StackedItemKit)
        {
            var stackedItem = ItemCatalog.GetItemFromId(itemId);

            if (stackedItem == null)
            {
                Logger.LogError("Unknown item with id {itemId}", itemId);
                continue;
            }

            for (var i = 0; i < ItemRConfig.AmountToStack; i++)
                items.Add(stackedItem);
        }

        player.Character.AddKit(items, amount);

        player.SendUpdatedInventory();
    }
}
