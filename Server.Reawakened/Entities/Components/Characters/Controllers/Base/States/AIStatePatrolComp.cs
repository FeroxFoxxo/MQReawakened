using A2m.Server;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStatePatrolComp : BaseAIState<AIStatePatrol>
{
    public override string StateName => "AIStatePatrol";

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

    public ServerRConfig ServerRConfig { get; set; }

    // Provide Initial Position
    public override ExtLevelEditor.ComponentSettings GetSettings()
    {
        var backPlaneZValue = ParentPlane == ServerRConfig.BackPlane ?
                      ServerRConfig.Planes[ServerRConfig.FrontPlane] :
                       ServerRConfig.Planes[ServerRConfig.BackPlane];

        return [Position.X.ToString(), Position.Y.ToString(), backPlaneZValue.ToString()];
    }
}
