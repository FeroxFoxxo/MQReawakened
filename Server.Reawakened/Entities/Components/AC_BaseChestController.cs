using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public abstract class AC_BaseChestController<T> : Component<T> where T : BaseChestController
{
    public bool IsLoyaltyChest => ComponentData.IsLoyaltyChest;
    public bool IsLootByPlayerLevel => ComponentData.IsLootByPlayerLevel;
    public bool IsInviteOnly => ComponentData.IsInviteOnly;
    public string TimedEventName => ComponentData.TimedEventName;
}
