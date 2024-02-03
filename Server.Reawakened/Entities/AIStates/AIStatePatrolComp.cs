using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.AIStates.SyncEvents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using System;
using Thrift.Protocol;
using UnityEngine;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.AIStates;
public class AIStatePatrolComp : Component<AIStatePatrol>
{
    public Vector2 Patrol1 => ComponentData.Patrol1;

    public Vector2 Patrol2 => ComponentData.Patrol2;

    public int SinusPathNbHalfPeriod => ComponentData.SinusPathNbHalfPeriod;

    public float MovementSpeed => ComponentData.MovementSpeed;

    public float IdleDurationAtTurnAround => ComponentData.IdleDurationAtTurnAround;

    public float DetectionRange => ComponentData.DetectionRange;

    public float MinimumRange => ComponentData.MinimumRange;

    public float MinimumTimeBeforeDetection => ComponentData.MinimumTimeBeforeDetection;

    public float MaximumYDifferenceOnDetection => ComponentData.MaximumYDifferenceOnDetection;

    public bool RayCastDetection => ComponentData.RayCastDetection;

    public bool DetectOnlyInPatrolZone => ComponentData.DetectOnlyInPatrolZone;

    public float PatrolZoneSizeOffset => ComponentData.PatrolZoneSizeOffset;

    public int AttackingEnemyId;
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public GameObjectComponents PreviousState = [];

    public ILogger<AIStatePatrolComp> Logger { get; set; }
    public int Health = 50;

    public override void InitializeComponent()
    {
        RunPlacement();
        //TimerThread.DelayCall(SpikerTest, null, TimeSpan.FromSeconds(15), TimeSpan.Zero, 1);
        base.InitializeComponent();
    }
    public override void RunSyncedEvent(SyncEvent syncEvent, Player player) => base.RunSyncedEvent(syncEvent, player);

    public void SpikerTest(object _)
    {
        var player = Room.Players.FirstOrDefault().Value;
        var enemy = Room.GetComponentsOfType<DrakeEnemyControllerComp>().Where(d => d.Key == Id).First().Value;

        foreach (var entity in Room.Entities.Values)
            foreach (var comp in entity)
                if (comp != enemy) return;

        var nextState = new GameObjectComponents()
                    {
                        {"AIStateSpikerAttack", new ComponentSettings()
                            {
                                player.TempData.Position.X.ToString()
                            }
                        }
                    };

        //Needs projectile implementation.

        GoToNextState(nextState);

        TimerThread.DelayCall(RunPatrol, null, TimeSpan.FromSeconds(5), TimeSpan.Zero, 1);
    }

    public void RunPlacement()
    {
        var enemyPlacement = Room.Entities[Id].First(x => x is AIStatePatrolComp) as AIStatePatrolComp;

        var nextState = new GameObjectComponents() {
                        {"AIStateDrakePlacement", new ComponentSettings()
                            {
                                enemyPlacement.Position.X.ToString(),
                                enemyPlacement.Position.Y.ToString(),
                                enemyPlacement.Position.Z.ToString(),
                                enemyPlacement.Position.X.ToString(),
                                enemyPlacement.Position.Y.ToString(),
                                enemyPlacement.Position.Z.ToString()
                            }
                        }
                    };

        GoToNextState(nextState);

        TimerThread.DelayCall(RunPatrol, null, TimeSpan.FromSeconds(1), TimeSpan.Zero, 1);
    }

    public void RunPatrol(object _)
    {
        foreach (var entity in Room.Entities.Values)
            foreach (var comp in entity)
                if (comp is DrakeEnemyControllerComp drake)
                drake.IsAttacking = false;

        var statePatrol = Room.Entities[Id].First(x => x is AIStatePatrolComp) as AIStatePatrolComp;

        var backPlaneZValue = statePatrol.ParentPlane == ServerRConfig.IsBackPlane[false] ? 0 : 20;

        //if (statePatrol.ParentPlane == ServerRConfig.IsBackPlane[false])
        //    statePatrol.Position.X -= 3; //Enemies on front plane need to be slightly adjusted on X axis due to random bug.

        var nextState = new GameObjectComponents() {
            {"AIStatePatrol", new ComponentSettings()
                {statePatrol.Position.X.ToString(),
                statePatrol.Position.Y.ToString(),
                backPlaneZValue.ToString()}
            }
        };

        GoToNextState(nextState);
    }

    public void GoToNextState(GameObjectComponents NewState)
    {
        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = PreviousState,
            GoToStates = NewState
        };

        PreviousState = NewState;

        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
    }
}
