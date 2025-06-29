using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderTeaserRetreatComp : BaseAIState<AIStateSpiderTeaserRetreat, AI_State>
{
    public override string StateName => "AIStateSpiderTeaserRetreat";

    public float TransTime => ComponentData.TransTime;
    public float DieDuration => ComponentData.DieDuration;
    public float TalkDuration => ComponentData.TalkDuration;
    public int DoorToOpenID => ComponentData.DoorToOpenID;

    public float ShakeDelay => ComponentData.ShakeDelay;

    public override AI_State GetInitialAIState() => new(
        [
            new (TransTime, "Transition"),
            new (TalkDuration, "Talk"),
            new (DieDuration - ShakeDelay, "Die"),
            new (ShakeDelay, "Shake")
        ], loop: false);

    public void Transition() => Logger.LogTrace("Transition called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Talk() => Logger.LogTrace("Talk called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Die()
    {
        Logger.LogTrace("Die called for {StateName} on {PrefabName}", StateName, PrefabName);

        OpenDoor();
    }

    public void Shake() => Logger.LogTrace("Shake called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void OpenDoor()
    {
        foreach (var trigReceiver in Room.GetEntitiesFromId<TriggerReceiverComp>(DoorToOpenID.ToString()))
            trigReceiver.Trigger(true, Id);
    }
}
