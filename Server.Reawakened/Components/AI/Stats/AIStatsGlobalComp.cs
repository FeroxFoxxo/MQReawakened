using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.Models;

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

    public AI_Stats_Global Default;

    public void MixGlobalProperties(ClassCopier classCopier, GlobalProperties globalProps)
    {
        var baseType = typeof(AI_Stats_Global);
        Default = classCopier.GetClassAndInfo(baseType).Key as AI_Stats_Global;

        if (Global_viewOffsetY != Default.Global_viewOffsetY)
            globalProps.Global_ViewOffsetY = Global_viewOffsetY;

        if (Global_FrontDetectionRangeX != Default.Global_FrontDetectionRangeX)
            globalProps.Global_FrontDetectionRangeX = Global_FrontDetectionRangeX;

        if (Global_FrontDetectionRangeUpY != Default.Global_FrontDetectionRangeUpY)
            globalProps.Global_FrontDetectionRangeUpY = Global_FrontDetectionRangeUpY;

        if (Global_FrontDetectionRangeDownY != Default.Global_FrontDetectionRangeDownY)
            globalProps.Global_FrontDetectionRangeDownY = Global_FrontDetectionRangeDownY;

        if (Global_BackDetectionRangeX != Default.Global_BackDetectionRangeX)
            globalProps.Global_BackDetectionRangeX = Global_BackDetectionRangeX;

        if (Global_BackDetectionRangeUpY != Default.Global_BackDetectionRangeUpY)
            globalProps.Global_BackDetectionRangeUpY = Global_BackDetectionRangeUpY;

        if (Global_BackDetectionRangeDownY != Default.Global_BackDetectionRangeDownY)
            globalProps.Global_BackDetectionRangeDownY = Global_BackDetectionRangeDownY;

        if (Global_ShootOffsetX != Default.Global_ShootOffsetX)
            globalProps.Global_ShootOffsetX = Global_ShootOffsetX;

        if (Global_ShootOffsetY != Default.Global_ShootOffsetY)
            globalProps.Global_ShootOffsetY = Global_ShootOffsetY;

        if (Global_DetectionSourceOnPatrolLine != Default.Global_DetectionSourceOnPatrolLine)
            globalProps.Global_DetectionSourceOnPatrolLine = Global_DetectionSourceOnPatrolLine;

        if (Global_DetectionLimitedByPatrolLine != Default.Global_DetectionLimitedByPatrolLine)
            globalProps.Global_DetectionLimitedByPatrolLine = Global_DetectionLimitedByPatrolLine;

        if (Global_ShootingProjectilePrefabName != Default.Global_ShootingProjectilePrefabName)
            globalProps.Global_ShootingProjectilePrefabName = Global_ShootingProjectilePrefabName;

        if (Global_DisableCollision != Default.Global_DisableCollision)
            globalProps.Global_DisableCollision = Global_DisableCollision;

        if (Global_Script != Default.Global_Script)
            globalProps.Global_Script = Global_Script;

        if (Aggro_AttackBeyondPatrolLine != Default.Aggro_AttackBeyondPatrolLine)
            globalProps.Aggro_AttackBeyondPatrolLine = Aggro_AttackBeyondPatrolLine;
    }
    public GenericScriptPropertiesModel MixGenericProperties(ClassCopier classCopier, GenericScriptPropertiesModel properties)
    {
        var baseType = typeof(AI_Stats_Global);
        Default = classCopier.GetClassAndInfo(baseType).Key as AI_Stats_Global;

        var attackBehavior = GenericScript_AttackBehavior != Default.GenericScript_AttackBehavior ?
            GenericScript_AttackBehavior :
            Enum.GetName(properties.AttackBehavior);

        var awareBehavior = GenericScript_AwareBehavior != Default.GenericScript_AwareBehavior ?
            GenericScript_AwareBehavior :
            Enum.GetName(properties.AwareBehavior);

        var unawareBehavior = GenericScript_UnawareBehavior != Default.GenericScript_UnawareBehavior ?
            GenericScript_UnawareBehavior :
            Enum.GetName(properties.UnawareBehavior);

        var duration = GenericScript_AwareBehaviorDuration != Default.GenericScript_AwareBehaviorDuration ?
            GenericScript_AwareBehaviorDuration :
            properties.genericScript_AwareBehaviorDuration;

        var healthRegenAmount = GenericScript_HealthRegenerationAmount != Default.GenericScript_HealthRegenerationAmount ?
            GenericScript_HealthRegenerationAmount :
            properties.genericScript_HealthRegenerationAmount;

        var healthRegenFreq = GenericScript_HealthRegenerationFrequency != Default.GenericScript_HealthRegenerationFrequency ?
            GenericScript_HealthRegenerationFrequency :
            properties.genericScript_HealthRegenerationFrequency;

        return new GenericScriptPropertiesModel(
            Enum.Parse<StateType>(attackBehavior), Enum.Parse<StateType>(awareBehavior), Enum.Parse<StateType>(unawareBehavior),
            duration, healthRegenAmount, healthRegenFreq
        );
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
    }
}
