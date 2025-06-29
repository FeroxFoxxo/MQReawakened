using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderRetreatComp : BaseAIState<AIStateSpiderRetreat, AI_State>
{
    public override string StateName => "AIStateSpiderRetreat";

    public float TransTime => ComponentData.TransTime;
    public float DieDuration => ComponentData.DieDuration;
    public float TalkDuration => ComponentData.TalkDuration;
    public int DoorToOpenID => ComponentData.DoorToOpenID;

    public override AI_State GetInitialAIState() => new(
        [
            new (TransTime, "Transition"),
            new (TalkDuration, "Talk"),
            new (DieDuration, "Die")
        ], loop: false);

    public void Transition() => Logger.LogTrace("Transition called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Talk() => Logger.LogTrace("Talk called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Die()
    {
        Logger.LogTrace("Die called for {StateName} on {PrefabName}", StateName, PrefabName);

        OpenDoor();
    }

    public void OpenDoor()
    {
        foreach (var trigReceiver in Room.GetEntitiesFromId<TriggerReceiverComp>(DoorToOpenID.ToString()))
            trigReceiver.Trigger(true, Id);
    }
}
