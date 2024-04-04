using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Players.Models;

namespace Server.Reawakened.Entities.AbstractComponents;

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

    public bool CanActivateDailies(Player player, string dailyObjectId) => 
        !player.Character.CurrentCollectedDailies.ContainsKey(dailyObjectId) ||
            player.Character.CurrentCollectedDailies.Values.Any(x => x.GameObjectId == dailyObjectId &&
                x.LevelId == player.Room.LevelInfo.LevelId && DateTime.Now >= x.TimeOfHarvest + TimeSpan.FromDays(1));

    public DailiesModel SetDailyHarvest(string gameObjectId, int levelId, DateTime timeOfHarvest) => new()
    {
        GameObjectId = gameObjectId,
        LevelId = levelId,
        TimeOfHarvest = timeOfHarvest
    };
}
