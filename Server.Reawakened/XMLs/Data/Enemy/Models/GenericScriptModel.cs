using Server.Reawakened.Entities.Components.AI.Stats;

namespace Server.Reawakened.XMLs.Data.Enemy.Models;
public class GenericScriptModel(string attackBehavior, string awareBehavior, string unawareBehavior, float awareBehaviorDuration, int healthRegenAmount, int healthRegenFreq)
{
    private static readonly Default_AI_Stats_Global genericGlobalStats = new();

    public void ApplyGenericPropertiesFromModel(AIStatsGlobalComp globalStats)
    {
        if (genericGlobalStats.GenericScript_AttackBehavior == globalStats.GenericScript_AttackBehavior)
            globalStats.ComponentData.GenericScript_AttackBehavior = attackBehavior;

        if (genericGlobalStats.GenericScript_AwareBehavior == globalStats.GenericScript_AwareBehavior)
            globalStats.ComponentData.GenericScript_AwareBehavior = awareBehavior;

        if (genericGlobalStats.GenericScript_UnawareBehavior == globalStats.GenericScript_UnawareBehavior)
            globalStats.ComponentData.GenericScript_UnawareBehavior = unawareBehavior;

        if (genericGlobalStats.GenericScript_AwareBehaviorDuration == globalStats.GenericScript_AwareBehaviorDuration)
            globalStats.ComponentData.GenericScript_AwareBehaviorDuration = awareBehaviorDuration;

        if (genericGlobalStats.GenericScript_HealthRegenerationAmount == globalStats.GenericScript_HealthRegenerationAmount)
            globalStats.ComponentData.GenericScript_HealthRegenerationAmount = healthRegenAmount;
        
        if (genericGlobalStats.GenericScript_HealthRegenerationFrequency == globalStats.GenericScript_HealthRegenerationFrequency)
            globalStats.ComponentData.GenericScript_HealthRegenerationFrequency = healthRegenFreq;
    }
}