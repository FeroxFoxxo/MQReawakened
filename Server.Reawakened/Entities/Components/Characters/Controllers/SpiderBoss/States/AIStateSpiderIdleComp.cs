﻿using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderIdleComp : BaseAIState<AIStateSpiderIdle, AI_State>
{
    public override string StateName => "AIStateSpiderIdle";

    public float[] Durations => ComponentData.Durations;

    public override AI_State GetInitialAIState() => new (
        [
            new(Durations[0], "Done")
        ], loop: false);

    public void Done()
    {
        Logger.LogTrace("Done called for {StateName} on {PrefabName}", StateName, PrefabName);

        var controller = StateMachine as SpiderBossControllerComp;

        if (controller == null)
        {
            RunVenomState();
            return;
        }

        switch (controller.CurrentPhase)
        {
            case 0:
                Logger.LogTrace("Idle pick: Venom (phase=0)");
                RunVenomState();
                break;
            case 1:
                if (System.Random.Shared.Next(0, 4) == 0)
                {
                    Logger.LogTrace("Idle pick: VineThrow (phase=1)");
                    RunVineThrowState();
                }
                else if (System.Random.Shared.Next(0, 2) == 0)
                {
                    Logger.LogTrace("Idle pick: Webs (phase=1)");
                    RunWebsState();
                }
                else
                {
                    Logger.LogTrace("Idle pick: Venom (phase=1)");
                    RunVenomState();
                }

                break;
            default:
                var roll = System.Random.Shared.Next(0, 5);
                
                if (roll == 0)
                {
                    Logger.LogTrace("Idle pick: SwitchSide (phase=2)");
                    RunSwitchSide();
                }
                else if (roll <= 2)
                {
                    Logger.LogTrace("Idle pick: Webs (phase=2)");
                    RunWebsState();
                }
                else if (roll == 3)
                {
                    Logger.LogTrace("Idle pick: VineThrow (phase=2)");
                    RunVineThrowState();
                }
                else
                {
                    Logger.LogTrace("Idle pick: Venom (phase=2)");
                    RunVenomState();
                }

                break;
        }
    }

    public void RunVenomState()
    {
        AddNextState<AIStateSpiderVenomComp>();
        GoToNextState();
    }

    public void RunWebsState()
    {
        AddNextState<AIStateSpiderWebsComp>();
        GoToNextState();
    }

    public void RunVineThrowState()
    {
        AddNextState<AIStateSpiderVineThrowComp>();
        GoToNextState();
    }

    public void RunSwitchSide()
    {
        var switchState = Room.GetEntityFromId<AIStateSpiderSwichSideComp>(Id);
        var moveState = Room.GetEntityFromId<AIStateSpiderMoveComp>(Id);

        if (switchState == null || moveState == null)
        {
            RunVenomState();
            return;
        }

        var targetX = Mathf.Abs(Position.X - switchState.XLeft) < Mathf.Abs(Position.X - switchState.XRight)
            ? switchState.XRight
            : switchState.XLeft;

        moveState.TargetPosition = new Vector3(targetX, moveState.CeilingY, Position.Z);

        AddNextState<AIStateSpiderSwichSideComp>();
        AddNextState<AIStateSpiderMoveComp>();
        AddNextState<AIStateSpiderDropComp>();
        GoToNextState();
    }
}
