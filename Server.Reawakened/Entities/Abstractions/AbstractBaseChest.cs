using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Abstractions;

public abstract class AbstractBaseChest<T> : SyncedEntity<T> where T : BaseChestController
{
    public bool IsLoyaltyChest => EntityData.IsLoyaltyChest;
    public bool IsLootByPlayerLevel => EntityData.IsLootByPlayerLevel;
    public bool IsInviteOnly => EntityData.IsInviteOnly;
    public string TimedEventName => EntityData.TimedEventName;
}
