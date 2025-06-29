using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollRetreatComp : BaseAIState<AIStateTrollRetreat, AI_State>
{
    public override string StateName => "AIStateTrollRetreat";

    public float TransTime => ComponentData.TransTime;
    public float TalkDuration => ComponentData.TalkDuration;
    public float DieDuration => ComponentData.DieDuration;
    public float ShakeDelay => ComponentData.ShakeDelay;
    public int DoorToOpenID => ComponentData.DoorToOpenID;

    public override AI_State GetInitialAIState() => new(
        [
            new AIDataEvent(TransTime, "Transition"),
            new AIDataEvent(TalkDuration, "Talk"),
            new AIDataEvent(DieDuration - ShakeDelay, "Die"),
            new AIDataEvent(ShakeDelay, "Shake")
        ], loop: false);

    public void Transition() => Logger.LogTrace("Transition called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Talk() => Logger.LogTrace("Talk called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Die() {
        Logger.LogTrace("Die called for {StateName} on {PrefabName}", StateName, PrefabName);

        OpenDoor();
    }

    public void OpenDoor()
    {
        foreach (var trigReceiver in Room.GetEntitiesFromId<TriggerReceiverComp>(DoorToOpenID.ToString()))
            trigReceiver.Trigger(true, Id);
    }

    public void Shake() => Logger.LogTrace("Shake called for {StateName} on {PrefabName}", StateName, PrefabName);
}
