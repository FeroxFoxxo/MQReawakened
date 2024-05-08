using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;

public interface ITriggerComp
{
    public void RunTrigger(Player player);
    public void ResetTrigger();
    public void AddPhysicalInteractor(Player player, string interactionId);
    public void RemovePhysicalInteractor(Player player, string interactionId);
    public int GetPhysicalInteractorCount();
    public string[] GetPhysicalInteractorIds();
    bool IsActive();
}
