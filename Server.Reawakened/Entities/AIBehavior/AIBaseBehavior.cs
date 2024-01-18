using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.Entities.AIBehavior;
public class AIBaseBehavior
{
    public virtual void Start(AIProcessData aiData, float startTime, string[] args)
    {
    }

    public virtual bool Update(AIProcessData aiData, float time)
    {
        return false;
    }

    public virtual float GetBehaviorRatio(AIProcessData aiData, float time)
    {
        return 0f;
    }
}
