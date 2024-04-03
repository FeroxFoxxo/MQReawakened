using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.XMLs.Models.Enemy.Models;
public class GenericScriptPropertiesModel(
    StateType attackBehavior, StateType awareBehavior,
    StateType unawareBehavior, float awareBehaviorDuration,
    int healthRegenerationAmount, int healthRegenerationFrequency) :
    GenericScriptProperties(
        Enum.GetName(attackBehavior), Enum.GetName(awareBehavior),
        Enum.GetName(unawareBehavior), awareBehaviorDuration,
        healthRegenerationAmount, healthRegenerationFrequency)
{
    public StateType AttackBehavior { get; } = attackBehavior;
    public StateType AwareBehavior { get; } = awareBehavior;
    public StateType UnawareBehavior { get; } = unawareBehavior;
}
