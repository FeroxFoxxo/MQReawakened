using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderEntranceComp : BaseAIState<AIStateSpiderEntrance, AI_State>
{
    public override string StateName => "AIStateSpiderEntrance";

    public float DelayBeforeEntranceDuration => ComponentData.DelayBeforeEntranceDuration;
    public float EntranceDuration => ComponentData.EntranceDuration;
    public float IntroDuration => ComponentData.IntroDuration;

    private SpiderBossControllerComp _spiderBossController;

    private float _entranceStartY;
    private float _entranceEndY;

    public override AI_State GetInitialAIState() => new(
        [
			new (DelayBeforeEntranceDuration, "Delay"),
			new (EntranceDuration, "Entrance"),
			new (IntroDuration - EntranceDuration - DelayBeforeEntranceDuration, "Talk"),
			new (1f, "Transition")
        ], loop: false);

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        _spiderBossController = Room.GetEntityFromId<SpiderBossControllerComp>(Id);
        _entranceEndY = Room.GetEntityFromId<AIStateSpiderPatrolComp>(Id).FromY;
        _entranceStartY = Room.GetEntityFromId<AIStateSpiderMoveComp>(Id).CeilingY;
        if (!_spiderBossController.Teaser)
        {
            Position.SetPosition(new vector3(Room.GetEntityFromId<AIStateSpiderSwichSideComp>(Id).XRight, _entranceStartY, Position.Z));
        }
    }

    public override void OnAIStateIn() => Position.SetPosition(new vector3(Room.GetEntityFromId<AIStateSpiderSwichSideComp>(Id).XRight, _entranceStartY, Position.Z));

    public override void Execute()
    {
        var eventRatio = 0f;

        if (State.CurrentEventName == "Entrance")
            eventRatio = State.EventRatio;
        else if (State.CurrentEventName is "Talk" or "Transition")
            eventRatio = 1f;

        Position.SetPosition(new vector3(Position.X, _entranceStartY + (_entranceEndY - _entranceStartY) * eventRatio, Position.Z));
    }

    public void Delay() => Logger.LogTrace("Delay called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Entrance() => Logger.LogTrace("Entrance called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Talk() => Logger.LogTrace("Talk called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Transition()
    {
        Logger.LogTrace("Transition called for {StateName} on {PrefabName}", StateName, PrefabName);
        RunExitEntrance();
    }

    private void RunExitEntrance()
    {
        AddNextState<AIStateSpiderDropComp>();
        GoToNextState();
    }

}
