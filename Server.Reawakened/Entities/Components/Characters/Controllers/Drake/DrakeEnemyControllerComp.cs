using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake;

public class DrakeEnemyControllerComp : Component<DrakeEnemyController>
{
    public TimerThread TimerThread { get; set; }
    public ILogger<DrakeEnemyControllerComp> Logger { get; set; }

    public bool IsAttacking = false;
    public string IdOfAttackingEnemy;
}
