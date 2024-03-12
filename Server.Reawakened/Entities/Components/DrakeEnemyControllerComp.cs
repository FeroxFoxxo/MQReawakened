using Server.Base.Timers.Services;
using Server.Reawakened.Entities.AIStates;
using static A2m.Server.ExtLevelEditor;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Extensions;
using Server.Base.Timers.Extensions;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Configs;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Components;

public class DrakeEnemyControllerComp : Component<DrakeEnemyController>
{
    private bool _isImmune;
    public TimerThread TimerThread { get; set; }
    public ILogger<DrakeEnemyControllerComp> Logger { get; set; }

    public bool IsAttacking = false;
    public string IdOfAttackingEnemy;
}
