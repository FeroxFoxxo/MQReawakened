using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.AIStates.SyncEvents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity.Utils;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using static A2m.Server.ExtLevelEditor;
using Server.Reawakened.Entities.AbstractComponents;
using A2m.Server;
using Server.Reawakened.Rooms.Models.Planes;
using Microsoft.Extensions.DependencyInjection;
using Server.Reawakened.XMLs.Bundles;
using UnityEngine;

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
}
