using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;

public abstract class AIBaseBehavior
{
    public abstract bool ShouldDetectPlayers { get; }

    private readonly AI_Behavior _behavior;

    public AIBaseBehavior() => _behavior = GetBehaviour();

    public void Start(AIProcessData aiData, float roomTime, string[] args) => _behavior.Start(aiData, roomTime, args);

    public bool TryUpdate(AIProcessData aiData, float roomTime, BehaviorEnemy enemy)
    {
        if (!_behavior.Update(aiData, roomTime))
        {
            NextState(enemy);
            return false;
        }

        return true;
    }

    public void Stop(AIProcessData aiData) => _behavior.Stop(aiData);

    public float GetBehaviorRatio(AIProcessData aiData, float roomTime) => _behavior.GetBehaviorRatio(aiData, roomTime);
    public void GetComebackPosition(AIProcessData aiData, ref float outPosX, ref float outPosY) => _behavior.GetComebackPosition(aiData, ref outPosX, ref outPosY);
    public void SetStats(AIProcessData aiData) => _behavior.SetStats(aiData);
    public bool MustDoComeback(AIProcessData aiData) => _behavior.MustDoComeback(aiData, _behavior);

    public virtual string[] GetInitArgs() => _behavior.GetInitArgs() ?? ([]);

    protected abstract AI_Behavior GetBehaviour();

    public abstract void NextState(BehaviorEnemy enemy);

    public abstract object[] GetData();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(';');

        foreach (var obj in GetData())
            sb.Append(obj is bool booleanValue ? booleanValue ? 1 : 0 : obj);

        return sb.ToString();
    }
}
