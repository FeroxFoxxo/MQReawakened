using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;

public abstract class AIBaseBehavior
{
    public abstract bool ShouldDetectPlayers { get; }

    public readonly BehaviorEnemy Enemy;

    private readonly AI_Behavior _behavior;

    public abstract StateType State { get; }

    public AIBaseBehavior(BehaviorEnemy behaviorEnemy)
    {
        Enemy = behaviorEnemy;
        _behavior = GetBehaviour();
    }

    public void Start() => _behavior.Start(Enemy.AiData, Enemy.Room.Time, GetStartArgsString().Split('`'));

    public bool TryUpdate()
    {
        if (!_behavior.Update(Enemy.AiData, Enemy.Room.Time))
        {
            NextState();
            return false;
        }

        return true;
    }

    public void Stop() => _behavior.Stop(Enemy.AiData);

    public float GetBehaviorRatio(float roomTime) => _behavior.GetBehaviorRatio(Enemy.AiData, roomTime);
    public void GetComebackPosition(ref float outPosX, ref float outPosY) => _behavior.GetComebackPosition(Enemy.AiData, ref outPosX, ref outPosY);
    public void SetStats() => _behavior.SetStats(Enemy.AiData);
    public bool MustDoComeback() => _behavior.MustDoComeback(Enemy.AiData, _behavior);

    public AI_Behavior GetBehaviour()
    {
        var typeName = Enum.GetName(State);
        var properties = AiPropertiesFactory.create(typeName, GetStartArgsString());
        return EnemyBehaviorFactory.create(typeName, properties);
    }

    public abstract void NextState();

    public abstract object[] GetProperties();
    public abstract object[] GetStartArgs();

    public string GetPropertiesString()
    {
        var sb = new SeparatedStringBuilder(';');

        foreach (var obj in GetProperties())
            sb.Append(obj is bool booleanValue ? booleanValue ? 1 : 0 : obj);

        return sb.ToString();
    }

    public string GetStartArgsString()
    {
        var sb = new SeparatedStringBuilder('`');

        foreach (var obj in GetStartArgs())
            sb.Append(obj is bool booleanValue ? booleanValue ? 1 : 0 : obj);

        return sb.ToString();
    }
}
