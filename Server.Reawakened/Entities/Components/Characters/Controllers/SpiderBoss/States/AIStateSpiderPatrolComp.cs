using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderPatrolComp : BaseAIState<AIStateSpiderPatrol>
{
    public float[] MovementSpeed => ComponentData.MovementSpeed;
    public float[] IdleDurationAtTurnAround => ComponentData.IdleDurationAtTurnAround;
    public float FromY => ComponentData.FromY;
    public float ToY => ComponentData.ToY;
    public float[] MinPatrolTime => ComponentData.MinPatrolTime;
    public float[] MaxPatrolTime => ComponentData.MaxPatrolTime;
}
