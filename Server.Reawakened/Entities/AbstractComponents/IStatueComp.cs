using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.AbstractComponents;

public interface IStatueComp
{
    public List<string> CurrentPhysicalInteractors { get; set; }
    public void RunSyncedEvent(SyncEvent syncEvent, Player player);
}
