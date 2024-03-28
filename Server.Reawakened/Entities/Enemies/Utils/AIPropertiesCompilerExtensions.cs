using Server.Reawakened.Entities.Enemies.EnemyAI;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Entities.Enemies.Utils;
public static class AIPropertiesCompilerExtensions
{
    public static string CreateBehaviorString(this BehaviorEnemy enemy, string name)
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
            case "Bomber":
                return CreateBomber(enemy);
            case "Grenadier":
                return CreateGrenadier(enemy);
            case "Stomper":
                return CreateStomper(enemy);
            case "Stinger":
                return CreateStinger(enemy);
            default:
                break;
        }
        return string.Empty;
    }

    private static string CreateLookAround(BehaviorEnemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "lookTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "startDirection"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "forceDirection"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "initialProgressRatio"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("LookAround", "snapOnGround"));

        return sb.ToString();
    }

    private static string CreatePatrol(BehaviorEnemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("Patrol", "speed"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Patrol", "smoothMove"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Patrol", "endPathWaitTime"));
        sb.Append(enemy.Generic.PatrolX);
        sb.Append(enemy.Generic.PatrolY);
        sb.Append(enemy.Generic.Patrol_ForceDirectionX);
        sb.Append(enemy.Generic.Patrol_InitialProgressRatio);

        return sb.ToString();
    }

    private static string CreateComeBack(BehaviorEnemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("ComeBack", "speed"));

        return sb.ToString();
    }

    private static string CreateAggro(BehaviorEnemy enemy)
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

    private static string CreateShooting(BehaviorEnemy enemy)
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

        return sb.ToString();
    }

    private static string CreateBomber(BehaviorEnemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("Bomber", "inTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Bomber", "loopTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Bomber", "bombRadius"));

        return sb.ToString();
    }

    private static string CreateGrenadier(BehaviorEnemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("Grenadier", "inTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Grenadier", "loopTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Grenadier", "outTime"));
        sb.Append(Convert.ToBoolean(enemy.BehaviorList.GetBehaviorStat("Grenadier", "isTracking")) ? 1 : 0);
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Grenadier", "projCount"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Grenadier", "projSpeed"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Grenadier", "maxHeight"));

        return sb.ToString();
    }

    private static string CreateStomper(BehaviorEnemy enemy)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stomper", "attackTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stomper", "impactTime"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stomper", "damageDistance"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Stomper", "damageOffset"));

        return sb.ToString();
    }

    private static string CreateStinger(BehaviorEnemy enemy)
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

    public static string CreateResources(this List<EnemyResourceModel> resources)
    {
        var assetList = new SeparatedStringBuilder('+');
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
