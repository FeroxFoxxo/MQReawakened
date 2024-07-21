using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;

public abstract class AIBaseBehavior
{
    public abstract bool ShouldDetectPlayers { get; }

    public readonly BehaviorEnemy Enemy;
    public readonly AI_Behavior Behavior;
    public readonly StateType State;

    public AIBaseBehavior(BehaviorEnemy behaviorEnemy, StateType stateType)
    {
        Enemy = behaviorEnemy;

        State = stateType;

        var typeName = Enum.GetName(State);
        var properties = AiPropertiesFactory.create(typeName, GetProperties().ToString());
        Behavior = EnemyBehaviorFactory.create(typeName, properties);
    }

    public void Start() => Behavior.Start(Enemy.AiData, Enemy.Room.Time, GetStartArgsArray());

    public bool TryUpdate()
    {
        if (!Behavior.Update(Enemy.AiData, Enemy.Room.Time))
        {
            NextState();
            return false;
        }

        return true;
    }

    public void Stop() => Behavior.Stop(Enemy.AiData);

    public float GetBehaviorRatio(float roomTime) => Behavior.GetBehaviorRatio(Enemy.AiData, roomTime);
    public void GetComebackPosition(ref float outPosX, ref float outPosY) => Behavior.GetComebackPosition(Enemy.AiData, ref outPosX, ref outPosY);
    public void SetStats() => Behavior.SetStats(Enemy.AiData);
    public bool MustDoComeback() => Behavior.MustDoComeback(Enemy.AiData, Behavior);

    public abstract void NextState();

    public virtual float GetBehaviorTime() => 1.0f;

    public abstract AiProperties GetProperties();
    public abstract object[] GetStartArgs();

    public string[] GetStartArgsArray()
    {
        var objs = new List<string>();

        foreach (var obj in GetStartArgs())
            objs.Add((obj is bool booleanValue ? booleanValue ? 1 : 0 : obj).ToString());

        return [.. objs];
    }

    public string GetStartArgsString()
    {
        var sb = new SeparatedStringBuilder('`');

        foreach (var obj in GetStartArgsArray())
            sb.Append(obj);

        return sb.ToString();
    }
}
