using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;

public abstract class AIBaseBehavior
{
    public abstract bool ShouldDetectPlayers { get; }

    protected readonly AI_Behavior _behaviour;
    protected readonly AIProcessData _aiData;
    protected readonly Room _room;

    public AIBaseBehavior(AIProcessData aiData, Room room)
    {
        _aiData = aiData;
        _room = room;

        var typeName = Enum.GetName(GetStateType());
        var properties = AiPropertiesFactory.create(typeName, GetProperties().ToString());
        _behaviour = EnemyBehaviorFactory.create(typeName, properties);
    }

    public void Start() => _behaviour.Start(_aiData, _room.Time, GetStartArgsArray());

    public bool TryUpdate()
    {
        if (!_behaviour.Update(_aiData, _room.Time))
        {
            NextState();
            return false;
        }

        return true;
    }

    public void Stop() => _behaviour.Stop(_aiData);

    public float GetBehaviorRatio(float roomTime) => _behaviour.GetBehaviorRatio(_aiData, roomTime);
    public void GetComebackPosition(ref float outPosX, ref float outPosY) => _behaviour.GetComebackPosition(_aiData, ref outPosX, ref outPosY);
    public void SetStats() => _behaviour.SetStats(_aiData);
    public bool MustDoComeback() => _behaviour.MustDoComeback(_aiData, _behaviour);

    public abstract StateType GetStateType();
    public abstract void NextState();

    public virtual float GetBehaviorTime() => 1.0f;

    public abstract AiProperties GetProperties();
    public abstract object[] GetStartArgs();

    private string[] GetStartArgsArray()
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

    protected static T Fallback<T>(T loaded, T fallback)
        => EqualityComparer<T>.Default.Equals(loaded, default) ? fallback : loaded;
}
