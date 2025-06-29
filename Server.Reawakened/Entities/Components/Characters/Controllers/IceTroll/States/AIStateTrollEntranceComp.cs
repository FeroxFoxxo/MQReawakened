using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollEntranceComp : BaseAIState<AIStateTrollEntrance, AI_State>
{
    public override string StateName => "AIStateTrollEntrance";

    public float DelayBeforeEntranceDuration => ComponentData.DelayBeforeEntranceDuration;
    public float EntranceDuration => ComponentData.EntranceDuration;
    public float IntroDuration => ComponentData.IntroDuration;


    private Vector3 _entranceEndPosition;

    private Vector3 _entranceStartPosition;

    private float _moveFactorHack;

    public override AI_State GetInitialAIState() => new(
        [
            new (DelayBeforeEntranceDuration, "Delay"),
            new (EntranceDuration, "Entrance"),
            new (IntroDuration - EntranceDuration - DelayBeforeEntranceDuration, "Talk"),
            new (1f, "Transition")
        ], loop: false);

    public override void StateIn() {
        _entranceEndPosition = Position.ToUnityVector3();
        _entranceStartPosition = _entranceEndPosition + new Vector3(10f, 0f, 0f);
        Position.SetPosition(_entranceStartPosition);
    }

    public override void Execute()
    {
        var diff = _entranceEndPosition - _entranceStartPosition;
        var ratio = (Room.Time - _stateStartTime - DelayBeforeEntranceDuration) / EntranceDuration;
        ratio = (!(ratio > 1f)) ? ratio : 1f;
        var position = _entranceStartPosition + diff * ratio * _moveFactorHack;
        Position.SetPosition(position);
    }

    public void Delay() => Logger.LogTrace("Delay called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Entrance()
    {
        Logger.LogTrace("Entrance called for {StateName} on {PrefabName}", StateName, PrefabName);

        _moveFactorHack = 1f;
    }

    public void Talk() => Logger.LogTrace("Talk called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Transition()
    {
        Logger.LogTrace("Transition called for {StateName} on {PrefabName}", StateName, PrefabName);

        RunExitEntrance();
    }

    private void RunExitEntrance()
    {
        AddNextState<AIStateTrollIdleComp>();
        GoToNextState();
    }
}
