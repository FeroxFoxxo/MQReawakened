using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorGrenadier(GrenadierState grenadierState) : AIBaseBehavior
{
    public AI_Behavior_Grenadier GrenadierBehavior = new(
        grenadierState.GInTime, grenadierState.GLoopTime,
        grenadierState.GOutTime, grenadierState.IsTracking,
        grenadierState.ProjCount, grenadierState.ProjSpeed,
        grenadierState.MaxHeight
    );

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => GrenadierBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => GrenadierBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => GrenadierBehavior.GetBehaviorRatio(aiData, roomTime);

    public override StateTypes GetBehavior() => StateTypes.Grenadier;
}
