using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Entities.Enemies.Utils;
public static class AIPropertiesSpawnerExtensions
{
    public static string CreateBehaviorString(this BaseSpawnerControllerComp spawner, string name)
    {
        switch (name)
        {
            case "LookAround":
                return CreateLookAround(spawner);
            case "Patrol":
                return CreatePatrol(spawner);
            case "ComeBack":
                return CreateComeBack(spawner);
            case "Aggro":
                return CreateAggro(spawner);
            case "Shooting":
                return CreateShooting(spawner);
            case "Bomber":
                return CreateBomber(spawner);
            case "Grenadier":
                return CreateGrenadier(spawner);
            case "Stomper":
                return CreateStomper(spawner);
            case "Stinger":
                return CreateStinger(spawner);
            default:
                break;
        }
        return string.Empty;
    }

    private static string CreateLookAround(BaseSpawnerControllerComp spawner)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(spawner.BehaviorList.GetBehaviorStat("LookAround", "lookTime"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("LookAround", "startDirection"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("LookAround", "forceDirection"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("LookAround", "initialProgressRatio"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("LookAround", "snapOnGround"));

        return sb.ToString();
    }

    private static string CreatePatrol(BaseSpawnerControllerComp spawner)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(spawner.BehaviorList.GetBehaviorStat("Patrol", "speed"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Patrol", "smoothMove"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Patrol", "endPathWaitTime"));
        sb.Append(spawner.Generic.PatrolX);
        sb.Append(spawner.Generic.PatrolY);
        sb.Append(spawner.Generic.Patrol_ForceDirectionX);
        sb.Append(spawner.Generic.Patrol_InitialProgressRatio);

        return sb.ToString();
    }

    private static string CreateComeBack(BaseSpawnerControllerComp spawner)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(spawner.BehaviorList.GetBehaviorStat("ComeBack", "speed"));

        return sb.ToString();
    }

    private static string CreateAggro(BaseSpawnerControllerComp spawner)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(spawner.BehaviorList.GetBehaviorStat("Aggro", "speed"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Aggro", "moveBeyondTargetDistance"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Aggro", "stayOnPatrolPath"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Aggro", "attackBeyondPatrolLine"));
        sb.Append(Convert.ToInt32(spawner.BehaviorList.GetBehaviorStat("Aggro", "attackBeyondPatrolLine")) > 0 ? 1 : 0);
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Aggro", "detectionRangeUpY"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Aggro", "detectionRangeDownY"));

        return sb.ToString();
    }

    private static string CreateShooting(BaseSpawnerControllerComp spawner)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "nbBulletsPerRound"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "fireSpreadAngle"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "delayBetweenBullet"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "delayShoot_Anim"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "nbFireRounds"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "delayBetweenFireRound"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "startCoolDownTime"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "endCoolDownTime"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "projectileSpeed"));
        sb.Append(Convert.ToBoolean(spawner.BehaviorList.GetBehaviorStat("Shooting", "fireSpreadClockwise")) ? 1 : 0);
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Shooting", "fireSpreadStartAngle"));

        return sb.ToString();
    }

    private static string CreateBomber(BaseSpawnerControllerComp spawner)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(spawner.BehaviorList.GetBehaviorStat("Bomber", "inTime"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Bomber", "loopTime"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Bomber", "bombRadius"));

        return sb.ToString();
    }

    private static string CreateGrenadier(BaseSpawnerControllerComp spawner)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(spawner.BehaviorList.GetBehaviorStat("Grenadier", "inTime"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Grenadier", "loopTime"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Grenadier", "outTime"));
        sb.Append(Convert.ToBoolean(spawner.BehaviorList.GetBehaviorStat("Grenadier", "isTracking")) ? 1 : 0);
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Grenadier", "projCount"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Grenadier", "projSpeed"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Grenadier", "maxHeight"));

        return sb.ToString();
    }

    private static string CreateStomper(BaseSpawnerControllerComp spawner)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stomper", "attackTime"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stomper", "impactTime"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stomper", "damageDistance"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stomper", "damageOffset"));

        return sb.ToString();
    }

    private static string CreateStinger(BaseSpawnerControllerComp spawner)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stinger", "speedForward"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stinger", "speedBackward"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stinger", "inDurationForward"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stinger", "attackDuration"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stinger", "damageAttackTimeOffset"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stinger", "inDurationBackward"));
        sb.Append(spawner.BehaviorList.GetBehaviorStat("Stinger", "damageDistance"));

        return sb.ToString();
    }
}
