using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;

public abstract class AIBaseBehavior
{
    private AI_Behavior _behavior;

    public abstract float ResetTime { get; }

    public AIBaseBehavior() => SetBehaviour();

    public void Start(ref AIProcessData aiData, float roomTime, string[] args) => _behavior.Start(aiData, roomTime, args);
    public bool Update(ref AIProcessData aiData, float roomTime) => _behavior.Update(aiData, roomTime);
    public float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => _behavior.GetBehaviorRatio(aiData, roomTime);
    public void Stop(ref AIProcessData aiData) => _behavior.Stop(aiData);
    public void GetComebackPosition(AIProcessData aiData, ref float outPosX, ref float outPosY) => _behavior.GetComebackPosition(aiData, ref outPosX, ref outPosY);
    public void SetStats(AIProcessData aiData) => _behavior.SetStats(aiData);
    public bool MustDoComeback(AIProcessData aiData) => _behavior.MustDoComeback(aiData, _behavior);
    public virtual string[] GetInitArgs() => _behavior.GetInitArgs() ?? ([]);

    public void SetBehaviour() => _behavior = GetBehaviour();
    protected abstract AI_Behavior GetBehaviour();
    public abstract StateTypes GetBehavior();
}
