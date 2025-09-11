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
        if (enemy.CurrentBehavior is AIBehaviorStinger stinger)
        {
            var stingerProperties = stinger.GetProperties() as StingerProperties;
            enemy.Room.ExplodeBomb(null, enemy.Position.ToUnityVector3(), stingerProperties.damageDistance, enemy.GetDamage(), Elemental.Standard, enemy.ServerRConfig, enemy.TimerThread);
        }
        else if (enemy.CurrentBehavior is AIBehaviorStomper stomper)
        {
            var stomperProperties = stomper.GetProperties() as StomperProperties;
            enemy.Room.ExplodeBomb(null, new Vector3(enemy.Position.X + (direction > 0 ? 1 : -1) * stomperProperties.damageOffset,
                enemy.Position.Y, enemy.Position.Z), stomperProperties.damageDistance, enemy.GetDamage(), Elemental.Standard, enemy.ServerRConfig, enemy.TimerThread);
        }
        else if (enemy.CurrentBehavior is AIBehaviorBomber bomber)
        {
            var bomberProperties = bomber.GetProperties() as BomberProperties;
            enemy.Room.ExplodeBomb(null, enemy.Position.ToUnityVector3(), bomberProperties.bombRadius, enemy.GetDamage(), Elemental.Standard, enemy.ServerRConfig, enemy.TimerThread);
            enemy.Damage(null, 99999);
        }
        else
            LogFacade.error("Running unimplemented AI method 'bomb' (from Bomber.cs)");
    }
}
