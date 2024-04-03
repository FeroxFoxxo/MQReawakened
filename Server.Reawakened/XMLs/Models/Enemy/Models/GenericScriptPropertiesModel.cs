using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.XMLs.Models.Enemy.Models;
public class GenericScriptPropertiesModel(
    StateTypes attackBehavior, StateTypes awareBehavior,
    StateTypes unawareBehavior, float awareBehaviorDuration,
    int healthRegenerationAmount, int healthRegenerationFrequency) :
    GenericScriptProperties(
        Enum.GetName(attackBehavior), Enum.GetName(awareBehavior),
        Enum.GetName(unawareBehavior), awareBehaviorDuration,
        healthRegenerationAmount, healthRegenerationFrequency)
{
    public StateTypes AttackBehavior { get; } = attackBehavior;
    public StateTypes AwareBehavior { get; } = awareBehavior;
    public StateTypes UnawareBehavior { get; } = unawareBehavior;
}
