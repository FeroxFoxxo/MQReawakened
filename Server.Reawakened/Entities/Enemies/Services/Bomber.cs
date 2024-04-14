using A2m.Server;
using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Players.Extensions;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Bomber(BehaviorEnemy enemy) : IBomber
{
    public override void bomb(int direction)
    {
        if (enemy.AiBehavior is AIBehaviorStinger stinger)
            enemy.Room.ExplodeBomb(null, enemy.Position, stinger.StingerDamageDistance, enemy.GetDamage(), Elemental.Standard, enemy.TimerThread);
        else if(enemy.AiBehavior is AIBehaviorStomper stomper)
            enemy.Room.ExplodeBomb(null, new Vector3(enemy.Position.x + (direction > 0 ? 1 : -1) * stomper.DamageOffset, enemy.Position.y, enemy.Position.z),
                stomper.DamageDistance, enemy.GetDamage(), Elemental.Standard, enemy.TimerThread);
        else if (enemy.AiBehavior is AIBehaviorBomber bomber)
            enemy.Room.ExplodeBomb(null, enemy.Position, bomber.BombRadius, enemy.GetDamage(), Elemental.Standard, enemy.TimerThread);
        else
            LogFacade.error("Running unimplemented AI method 'bomb' (from Bomber.cs)");
    }
}
