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
        sb.Append(enemy.Generic.Aggro_UseAttackBeyondPatrolLine ? 1 : 0);
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Aggro", "detectionRangeUpY"));
        sb.Append(enemy.BehaviorList.GetBehaviorStat("Aggro", "detectionRangeDownY"));

        return sb.ToString();
    }

    public string CreateResource(string asset)
    {
        return string.Empty;
    }
}
