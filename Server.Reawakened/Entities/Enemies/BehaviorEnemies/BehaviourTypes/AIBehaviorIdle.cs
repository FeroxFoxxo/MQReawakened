using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
public class AIBehaviorIdle : AIBaseBehavior
{
    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Idle();

    public override StateTypes GetBehavior() => StateTypes.Idle;

    public override object[] GetData() => [];
}
