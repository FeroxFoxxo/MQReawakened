using Server.Reawakened.Rooms.Models.Entities;

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
}
