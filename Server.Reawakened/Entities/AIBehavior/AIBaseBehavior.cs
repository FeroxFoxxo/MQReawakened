using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.Entities.AIBehavior;
public class AIBaseBehavior
{
    public virtual void Start(ref AIProcessData aiData, float startTime, string[] args)
    {
    }

    public virtual bool Update(ref AIProcessData aiData, float time) => false;

    public virtual float GetBehaviorRatio(ref AIProcessData aiData, float time) => 0f;

    public virtual void Stop(ref AIProcessData aiData) { }
}
