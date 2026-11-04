using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
public class EnemyControllerComp : BaseEnemyControllerComp<EnemyController>
{
    public void Damage(Player origin, int damage, bool aggro = true) => Enemy.Damage(origin, damage, aggro);
}
