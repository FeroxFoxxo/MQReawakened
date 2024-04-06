using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.XMLs.Data.Enemy.Models;
public class GenericScriptPropertiesModel(
    StateType attackBehavior, StateType awareBehavior,
    StateType unawareBehavior, float awareBehaviorDuration,
    int healthRegenerationAmount, int healthRegenerationFrequency) :
    GenericScriptProperties(
        Enum.GetName(attackBehavior), Enum.GetName(awareBehavior),
        Enum.GetName(unawareBehavior), awareBehaviorDuration,
        healthRegenerationAmount, healthRegenerationFrequency)
{
    public StateType AttackBehavior => attackBehavior;
    public StateType AwareBehavior => awareBehavior;
    public StateType UnawareBehavior => unawareBehavior;
}
