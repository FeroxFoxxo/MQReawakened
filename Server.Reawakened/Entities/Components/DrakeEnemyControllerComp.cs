using Server.Base.Timers.Services;
using Server.Reawakened.Rooms.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Server.Reawakened.Entities.Components;

public class DrakeEnemyControllerComp : Component<DrakeEnemyController>
{
    private bool _isImmune;
    public TimerThread TimerThread { get; set; }
    public ILogger<DrakeEnemyControllerComp> Logger { get; set; }

    public bool IsAttacking = false;
    public string IdOfAttackingEnemy;
}
