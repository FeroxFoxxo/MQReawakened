using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.AbstractComponents;

public interface ITriggerComp
{
    public List<string> CurrentPhysicalInteractors { get; set; }
    public void RunTrigger(Player player);
}
