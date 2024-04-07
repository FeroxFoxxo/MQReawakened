using A2m.Server;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorBomber(BomberState bomberState) : AIBaseBehavior
{
    public float InTime => bomberState.InTime;
    public float LoopTime => bomberState.LoopTime;
    public float BombRadius => bomberState.BombRadius;

    public override bool ShouldDetectPlayers => false;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Bomber(InTime, LoopTime);

    public override object[] GetData() => [InTime, LoopTime, BombRadius];

    public override void NextState(BehaviorEnemy enemy)
    {
        enemy.Room.ExplodeBomb(null, enemy.Position, BombRadius, enemy.GetDamage(), Elemental.Standard, enemy.TimerThread);
        enemy.Damage(enemy.EnemyController.MaxHealth, null);
    }
}
