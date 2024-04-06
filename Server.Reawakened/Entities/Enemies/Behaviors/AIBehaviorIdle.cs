﻿using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorIdle : AIBaseBehavior
{
    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Idle();

    public override StateType GetBehavior() => StateType.Idle;

    public override object[] GetData() => [];
}
