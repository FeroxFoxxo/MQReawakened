using static A2m.Server.ExtLevelEditor;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.DataComponentAccessors.Hampster.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Hamster.States;
public class AIStateHamsterAttackComp : BaseAIState<AIStateHamsterAttackMQR, AI_State_HamsterAttack>
{
    public override string StateName => "AIStateHamsterAttack";

    public float InTime => ComponentData.InTime;
    public float LoopTime => ComponentData.LoopTime;
    public float OutTime => ComponentData.OutTime;
    public float JumpHeight => ComponentData.JumpHeight;

    public vector3 TargetPosition = new (0f, 0f, 0f);
    
    public float _endTime;
    
    public override AI_State_HamsterAttack GetInitialAIState() => new (
        [
            new (InTime, "AttackIn"),
            new (LoopTime, "AttackLoop"),
            new (OutTime, "AttackOut")
        ], JumpHeight);

    public override ComponentSettings GetSettings() => [
        Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString(),
        TargetPosition.x.ToString(), (TargetPosition.y - JumpHeight).ToString(), TargetPosition.z.ToString()
    ];
    
    public override void InitializeComponent()
    {
        base.InitializeComponent();
        _endTime = -1;
    }

    public override void OnAIStateIn()
    {
        _endTime = Room.Time + InTime + LoopTime + OutTime - 0.05f;
        
        State.Init(Position.ToVector3(), TargetPosition);
    }

    public override void Execute()
    {
        Position.SetPosition(State.CurrentPosition);
        
        if (Room.Time >= _endTime)
        {
            AddNextState<AIStateWait2Comp>();
            GoToNextState();
        }
    }

    public void AttackIn() => Logger.LogTrace("AttackIn called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void AttackLoop() => Logger.LogTrace("AttackLoop called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void AttackOut() => Logger.LogTrace("AttackOut called for {StateName} on {PrefabName}", StateName, PrefabName);
}
