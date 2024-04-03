using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.XMLs.Models.Enemy.Models;
public class GenericScriptModel(StateType attackBehavior, StateType awareBehavior, StateType unawareBehavior, float awareBehaviorDuration, int healthRegenAmount, int healthRegenFreq)
{
    public GenericScriptPropertiesModel GenerateGenericPropertiesFromModel(ClassCopier classCopier, AIStatsGlobalComp globalStats)
    {
        var properties = new GenericScriptPropertiesModel(
            attackBehavior,
            awareBehavior,
            unawareBehavior,
            awareBehaviorDuration,
            healthRegenAmount,
            healthRegenFreq
        );

        // Breaks enemy behavior stats, commenting out for now
        properties = globalStats?.MixGenericProperties(classCopier, properties);

        return properties;
    }
}
