using Server.Reawakened.Entities.Components;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorPatrol(PatrolState patrolState, AIStatsGenericComp genericComp) : AIBaseBehavior
{
    public AI_Behavior_Patrol PatrolBehavior = new(patrolState.Speed, patrolState.EndPathWaitTime, genericComp.PatrolX,
        genericComp.PatrolY, genericComp.Patrol_ForceDirectionX, genericComp.Patrol_InitialProgressRatio);

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => PatrolBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => PatrolBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => PatrolBehavior.GetBehaviorRatio(aiData, roomTime);

    public override void Stop(ref AIProcessData aiData) => PatrolBehavior.Stop(aiData);

    public override void GetComebackPosition(AIProcessData aiData, ref float outPosX, ref float outPosY) => PatrolBehavior.GetComebackPosition(aiData, ref outPosX, ref outPosY);

    public override StateTypes GetBehavior() => StateTypes.Patrol;
}
