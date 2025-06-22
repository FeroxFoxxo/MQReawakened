using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;

public interface ITriggerComp
{
    void RunTrigger(Player player);
    void ResetTrigger();
    void AddPhysicalInteractor(Player player, string interactionId);
    void RemovePhysicalInteractor(Player player, string interactionId);
    int GetPhysicalInteractorCount();
    string[] GetPhysicalInteractorIds();
    bool IsActive();
}
