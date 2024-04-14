using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;

public interface ITriggerComp
{
    public void RunTrigger(Player player);
    public void ResetTrigger();
    public void AddPhysicalInteractor(Player player, string interactorId);
    public void RemovePhysicalInteractor(Player player, string interactorId);
    public int GetPhysicalInteractorCount();
    public string[] GetPhysicalInteractorIds();
    bool IsActive();
}
