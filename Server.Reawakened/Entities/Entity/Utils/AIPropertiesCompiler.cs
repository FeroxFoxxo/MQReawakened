using Server.Reawakened.Players.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.Entities.Entity.Utils;
public class AIPropertiesCompiler
{
    public string CreateBehaviorString(Enemy enemy, string name)
    {
        switch (name)
        {
            case "LookAround":
                return CreateLookAround(enemy);
            case "Patrol":
                return CreatePatrol(enemy);
            case "ComeBack":
                return CreateComeBack(enemy);
            case "Aggro":
                return CreateAggro(enemy);
            case "Shooting":
                return CreateShooting(enemy);
            case "Stomper":
                return CreateStomper(enemy);
            case "Stinger":
                return CreateStinger(enemy);
            default:
                break;
        }
        return string.Empty;
    }

    public string CreateLookAround(Enemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "lookTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "startDirection"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "forceDirection"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "initialProgressRatio"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "snapOnGround"));

        return sb.ToString();
    }

    public string CreatePatrol(Enemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("Patrol", "speed"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Patrol", "smoothMove"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Patrol", "endPathWaitTime"));
        sb.Append(enemy.Generic.Patrol_DistanceX);
        sb.Append(enemy.Generic.Patrol_DistanceY);
        sb.Append(enemy.Generic.Patrol_ForceDirectionX);
        sb.Append(enemy.Generic.Patrol_InitialProgressRatio);

        return sb.ToString();
    }

    public string CreateComeBack(Enemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("ComeBack", "speed"));

        return sb.ToString();
    }

    public string CreateAggro(Enemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("Aggro", "speed"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Aggro", "moveBeyondTargetDistance"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Aggro", "stayOnPatrolPath"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Aggro", "attackBeyondPatrolLine"));
        sb.Append(Convert.ToInt32(enemy.BehaviorList.GetBehaviorStat("Aggro", "attackBeyondPatrolLine")) > 0 ? 1 : 0);
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Aggro", "detectionRangeUpY"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Aggro", "detectionRangeDownY"));

        return sb.ToString();
    }

    public string CreateShooting(Enemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "nbBulletsPerRound"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "fireSpreadAngle"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "delayBetweenBullet"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "delayShoot_Anim"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "nbFireRounds"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "delayBetweenFireRound"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "startCoolDownTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "endCoolDownTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "projectileSpeed"));
        sb.Append(Convert.ToBoolean(enemy.BehaviorList.GetBehaviorStat("Shooting", "fireSpreadClockwise")) ? 1 : 0);
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Shooting", "fireSpreadStartAngle"));

        Console.WriteLine(sb.ToString());
        return sb.ToString();
    }

    public string CreateStomper(Enemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stomper", "attackTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stomper", "impactTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stomper", "damageDistance"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stomper", "damageOffset"));

        return sb.ToString();
    }

    public string CreateStinger(Enemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stinger", "speedForward"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stinger", "speedBackward"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stinger", "inDurationForward"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stinger", "attackDuration"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stinger", "damageAttackTimeOffset"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stinger", "inDurationBackward"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stinger", "damageDistance"));

        return sb.ToString();
    }

    public string CreateResources(List<EnemyResourceModel> resources)
    {
        SeparatedStringBuilder assetList = new SeparatedStringBuilder('+');
        string asset;

        if (resources.Count > 0)
        {
            foreach (var prefab in resources)
            {
                asset = string.Empty;
                asset = prefab.Type + '-' + prefab.Resource;
                assetList.Append(asset);
            }
            return assetList.ToString();
        }
        return string.Empty;
    }
}
