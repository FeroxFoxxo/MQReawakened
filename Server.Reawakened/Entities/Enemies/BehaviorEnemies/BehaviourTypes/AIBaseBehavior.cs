using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
public abstract class AIBaseBehavior
{
    public virtual void Start(ref AIProcessData aiData, float startTime, string[] args) { }

    public virtual bool Update(ref AIProcessData aiData, float time) => false;

    public virtual float GetBehaviorRatio(ref AIProcessData aiData, float time) => 0f;

    public virtual void Stop(ref AIProcessData aiData) { }

    public virtual bool MustDoComeback(AIProcessData aiData) => false;

    public virtual void GetComebackPosition(AIProcessData aiData, ref float outPosX, ref float outPosY) { }

    public abstract StateTypes GetBehavior();
}
