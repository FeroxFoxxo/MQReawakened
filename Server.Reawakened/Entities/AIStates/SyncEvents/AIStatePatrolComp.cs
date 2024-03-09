using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.AIStates.SyncEvents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.AIStates;
public class AIStatePatrolComp : Component<AIStatePatrol>
{
    public Vector2Model Patrol1 => new()
    {
        X = ComponentData.Patrol1.x,
        Y = ComponentData.Patrol1.y,
    };

    public Vector2Model Patrol2 => new()
    {
        X = ComponentData.Patrol2.x,
        Y = ComponentData.Patrol2.y,
    };
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
    public AI_State_Patrol AIState => ComponentData.State;

    public TimerThread TimerThread { get; set; }
    public GameObjectComponents PreviousState = [];
    public ServerRConfig ServerRConfig {  get; set; }
    public ILogger<AIStatePatrolComp> Logger { get; set; }

    public override void InitializeComponent()
    {
        Logger.LogInformation($"AIData: \n1) Patrol1 x:{Patrol1.X} y:{Patrol1.Y}\n2) Patrol2 x:{Patrol2.X} y:{Patrol2.Y}\n" +
            $"3) SinusPathNbHalfPeriod:{SinusPathNbHalfPeriod}\n4) MovementSpeed:{MovementSpeed}\n" +
            $"5) IdleDurationAtTurnAround:{IdleDurationAtTurnAround}\n6) DetectionRange:{DetectionRange}\n" +
            $"7) MinRange:{MinimumRange}\n8) MinTimeBeforeDetection:{MinimumTimeBeforeDetection}\n" +
            $"9) MaxYDifferenceOnDetecion:{MaximumYDifferenceOnDetection}\n10) RayCastDetection:{RayCastDetection}\n" +
            $"11) DetectionOnlyInPatrolZone:{DetectOnlyInPatrolZone}\n12) PatrolZoneSizeOffset:{PatrolZoneSizeOffset}\n" +
            $"State:{AIState}");

        RunPlacement();
    }
    //TimerThread.DelayCall(SpikerTest, null, TimeSpan.FromSeconds(15), TimeSpan.Zero, 1);

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player) => base.RunSyncedEvent(syncEvent, player);

    public void RunPlacement()
    {
        var nextState = new GameObjectComponents() {
                        {"AIStateDrakePlacement", new ComponentSettings()
                            {
                                Position.X.ToString(),
                                Position.Y.ToString(),
                                Position.Z.ToString(),
                                Position.X.ToString(),
                                Position.Y.ToString(),
                                Position.Z.ToString()
                            }
                        }
                    };

        GoToNextState(nextState);

        TimerThread.DelayCall(RunPatrol, null, TimeSpan.FromSeconds(1), TimeSpan.Zero, 1);
    }

    public void RunPatrol(object _)
    {
        Room.GetEntitiesFromId<DrakeEnemyControllerComp>(Id).First().IsAttacking = false;

        var statePatrol = Room.GetEntitiesFromId<AIStatePatrolComp>(Id).First();
        var backPlaneZValue = statePatrol.ParentPlane == "Plane0" ? ServerRConfig.FrontPlaneZ : ServerRConfig.BackPlaneZ;

        //if (statePatrol.ParentPlane == ServerRConfig.IsBackPlane[false])
        //    statePatrol.Position.X -= 3; Enemies on front plane need to be slightly adjusted on X axis due to random bug.

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

    public void SpikerTest(object _)
    {
        var player = Room.Players.FirstOrDefault().Value;
        var drake = Room.GetEntitiesFromId<DrakeEnemyControllerComp>(Id).First();

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
}
