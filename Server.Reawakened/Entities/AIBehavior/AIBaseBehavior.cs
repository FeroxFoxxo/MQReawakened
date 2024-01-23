namespace Server.Reawakened.Entities.AIBehavior;
public class AIBaseBehavior
{
    public virtual void Start(AIProcessData aiData, float startTime, string[] args)
    {
    }

    public virtual bool Update(AIProcessData aiData, float time) => false;

    public virtual float GetBehaviorRatio(AIProcessData aiData, float time) => 0f;
}
