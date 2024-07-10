using Server.Reawakened.Players;
using Server.Reawakened.Players.Models.Misc;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.GameObjects.Items.Abstractions;

public abstract class BaseChestControllerComp<T> : Component<T> where T : BaseChestController
{
    public bool IsLoyaltyChest => ComponentData.IsLoyaltyChest;
    public bool IsLootByPlayerLevel => ComponentData.IsLootByPlayerLevel;
    public bool IsInviteOnly => ComponentData.IsInviteOnly;
    public string TimedEventName => ComponentData.TimedEventName;
    public enum DailiesState
    {
        Collected = 0,
        Active = 1
    }

    public bool CanActivateDailies(Player player, string dailyHarvestId)
    {
        if (!player.Character.CurrentCollectedDailies.ContainsKey(dailyHarvestId) ||
            player.Character.CurrentCollectedDailies.TryGetValue(dailyHarvestId, out var dailyHarvest) &&
            dailyHarvest.GameObjectId == dailyHarvestId && dailyHarvest.LevelId == player.Room.LevelInfo.LevelId &&
            DateTime.Now >= dailyHarvest.TimeOfHarvest + TimeSpan.FromDays(1))
        {
            player.Character.CurrentCollectedDailies.Remove(dailyHarvestId);
            return true;
        }

        else
            return false;
    }

    public DailiesModel SetDailyHarvest(string gameObjectId, int levelId, DateTime timeOfHarvest) => new()
    {
        GameObjectId = gameObjectId,
        LevelId = levelId,
        TimeOfHarvest = timeOfHarvest
    };
}
