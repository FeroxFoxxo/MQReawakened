using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Components.AI.Stats;
public class AIStatsGlobalComp : Component<AI_Stats_Global>
{
    public float Global_viewOffsetY => ComponentData.Global_viewOffsetY;
    public float Global_FrontDetectionRangeX => ComponentData.Global_FrontDetectionRangeX;
    public float Global_FrontDetectionRangeUpY => ComponentData.Global_FrontDetectionRangeUpY;
    public float Global_FrontDetectionRangeDownY => ComponentData.Global_FrontDetectionRangeDownY;
    public float Global_BackDetectionRangeX => ComponentData.Global_BackDetectionRangeX;
    public float Global_BackDetectionRangeUpY => ComponentData.Global_BackDetectionRangeUpY;
    public float Global_BackDetectionRangeDownY => ComponentData.Global_BackDetectionRangeDownY;
    public float Global_ShootOffsetX => ComponentData.Global_ShootOffsetX;
    public float Global_ShootOffsetY => ComponentData.Global_ShootOffsetY;
    public bool Global_DetectionSourceOnPatrolLine => ComponentData.Global_DetectionSourceOnPatrolLine;
    public bool Global_DetectionLimitedByPatrolLine => ComponentData.Global_DetectionLimitedByPatrolLine;
    public string Global_ShootingProjectilePrefabName => ComponentData.Global_ShootingProjectilePrefabName;
    public bool Global_DisableCollision => ComponentData.Global_DisableCollision;
    public string Global_Script => ComponentData.Global_Script;

    public float Patrol_MoveSpeed => ComponentData.Patrol_MoveSpeed;
    public bool Patrol_SmoothMove => ComponentData.Patrol_SmoothMove;
    public float Patrol_EndPathWaitTime => ComponentData.Patrol_EndPathWaitTime;

    public float Aggro_AttackSpeed => ComponentData.Aggro_AttackSpeed;
    public float Aggro_MoveBeyondTargetDistance => ComponentData.Aggro_MoveBeyondTargetDistance;
    public bool Aggro_StayOnPatrolPath => ComponentData.Aggro_StayOnPatrolPath;
    public float Aggro_AttackBeyondPatrolLine => ComponentData.Aggro_AttackBeyondPatrolLine;

    public float ComeBack_MoveSpeed => ComponentData.ComeBack_MoveSpeed;

    public float LookAround_LookTime => ComponentData.LookAround_LookTime;
    public int LookAround_StartDirection => ComponentData.LookAround_StartDirection;
    public int LookAround_ForceDirection => ComponentData.LookAround_ForceDirection;
    public float LookAround_InitialProgressRatio => ComponentData.LookAround_InitialProgressRatio;
    public bool LookAround_SnapOnGround => ComponentData.LookAround_SnapOnGround;

    public float QuadWaveShooting_SingleWaveDuration => ComponentData.QuadWaveShooting_SingleWaveDuration;
    public float QuadWaveShooting_SingleWaveDistance => ComponentData.QuadWaveShooting_SingleWaveDistance;
    public float QuadWaveShooting_SingleWaveAmplitude => ComponentData.QuadWaveShooting_SingleWaveAmplitude;
    public float QuadWaveShooting_EndSingleWaveWaitTime => ComponentData.QuadWaveShooting_EndSingleWaveWaitTime;
    public float QuadWaveShooting_StartWaitTime => ComponentData.QuadWaveShooting_StartWaitTime;

    public float Shooting_DelayShot_Anim => ComponentData.Shooting_DelayShot_Anim;
    public int Shooting_NbBulletsPerRound => ComponentData.Shooting_NbBulletsPerRound;
    public float Shooting_FireSpreadAngle => ComponentData.Shooting_FireSpreadAngle;
    public float Shooting_DelayBetweenBullet => ComponentData.Shooting_DelayBetweenBullet;
    public int Shooting_NbFireRounds => ComponentData.Shooting_NbFireRounds;
    public float Shooting_DelayBetweenFireRound => ComponentData.Shooting_DelayBetweenFireRound;
    public float Shooting_StartCoolDownTime => ComponentData.Shooting_StartCoolDownTime;
    public float Shooting_EndCoolDownTime => ComponentData.Shooting_EndCoolDownTime;
    public float Shooting_ProjectileSpeed => ComponentData.Shooting_ProjectileSpeed;
    public bool Shooting_FireSpreadClockwise => ComponentData.Shooting_FireSpreadClockwise;
    public float Shooting_FireSpreadStartAngle => ComponentData.Shooting_FireSpreadStartAngle;

    public string GenericScript_AttackBehavior => ComponentData.GenericScript_AttackBehavior;
    public string GenericScript_AwareBehavior => ComponentData.GenericScript_AwareBehavior;
    public string GenericScript_UnawareBehavior => ComponentData.GenericScript_UnawareBehavior;
    public float GenericScript_AwareBehaviorDuration => ComponentData.GenericScript_AwareBehaviorDuration;
    public int GenericScript_HealthRegenerationAmount => ComponentData.GenericScript_HealthRegenerationAmount;
    public int GenericScript_HealthRegenerationFrequency => ComponentData.GenericScript_HealthRegenerationFrequency;

    public StateType AttackBehavior = StateType.Unknown;
    public StateType AwareBehavior = StateType.Unknown;
    public StateType UnawareBehavior = StateType.Unknown;

    public override void InitializeComponent()
    {
        AttackBehavior = Enum.Parse<StateType>(GenericScript_AttackBehavior, true);
        AwareBehavior = Enum.Parse<StateType>(GenericScript_AwareBehavior, true);
        UnawareBehavior = Enum.Parse<StateType>(GenericScript_UnawareBehavior, true);
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
    }

    public GlobalProperties GetGlobalProperties() =>
        new (
            Global_DetectionLimitedByPatrolLine,
            Global_BackDetectionRangeX,
            Global_viewOffsetY,
            Global_BackDetectionRangeUpY,
            Global_BackDetectionRangeDownY,
            Global_ShootOffsetX,
            Global_ShootOffsetY,
            Global_FrontDetectionRangeX,
            Global_FrontDetectionRangeUpY,
            Global_FrontDetectionRangeDownY,
            Global_Script,
            Global_ShootingProjectilePrefabName,
            Global_DisableCollision,
            Global_DetectionSourceOnPatrolLine,
            Aggro_AttackBeyondPatrolLine
        );
}
