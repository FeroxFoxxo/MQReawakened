using Microsoft.Extensions.Logging;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.AIStates;
public class AIStateDrakeAttackComp : Component<AIStateDrakeAttack>
{
    public AnimationClip AttackLoopAnim => ComponentData.AttackLoopAnim;

    public AnimationClip AttackOutAnim => ComponentData.AttackOutAnim;

    public AnimationClip StunAnim => ComponentData.StunAnim;

    public AnimationClip TauntAnim => ComponentData.TauntAnim;

    public AnimationClip FleeAnim => ComponentData.FleeAnim;
    public string AttackSound => ComponentData.AttackSound;
    public string TauntSound => ComponentData.TauntSound;
    public float RamSpeed => ComponentData.RamSpeed;
    public float AttackOutAnimDuration => ComponentData.AttackOutAnimDuration;
    public float StunDuration => ComponentData.StunDuration;
    public float FleeSpeed => ComponentData.FleeSpeed;
    public float ReloadDuration => ComponentData.ReloadDuration;
    public float TauntAnimDuration => ComponentData.TauntAnimDuration;

    public AI_State_DrakeAttack State => ComponentData.State;

    public ILogger<AIStatePatrolComp> Logger { get; set; }

    public override void InitializeComponent()
    {
        Console.WriteLine(TauntAnimDuration + " " + AttackOutAnimDuration + " " + TauntAnimDuration + " " + ReloadDuration);
    }
}
