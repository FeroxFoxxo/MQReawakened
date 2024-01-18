using Server.Reawakened.Entities.AIAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Server.Reawakened.Entities.AIBehavior;
public class AIBehavior_LookAround : AIBaseBehavior
{
    private float LookAround_LookTime;
    private float LookAround_InitialProgressRatio;
    private bool LookAround_SnapOnGround;
    private int _currentStep;
    private float _behaviorStartTime;
    private float _behaviorDuration;
    private float _currentRatio;
    private AI_Action_GoTo _goTo;

    public AIBehavior_LookAround(float LookAround_LookTime, float LookAround_InitialProgressRatio, bool LookAround_SnapOnGround)
    {
        this.LookAround_LookTime = LookAround_LookTime;
        this.LookAround_InitialProgressRatio = LookAround_InitialProgressRatio;
        this.LookAround_SnapOnGround = LookAround_SnapOnGround;
    }

    public override void Start(AIProcessData aiData, float startTime, string[] args)
    {
        _currentStep = 0;
        if (LookAround_LookTime == 0f)
        {
            LookAround_LookTime = 0.1f;
        }
        _behaviorDuration = 2f * LookAround_LookTime;
        _behaviorStartTime = startTime - LookAround_InitialProgressRatio * (_behaviorDuration / aiData.Sync_SpeedFactor);
        if (LookAround_SnapOnGround)
        {
            _goTo = new AI_Action_GoTo(ref aiData, aiData.Sync_PosX, aiData.Sync_PosY, aiData.Sync_PosX, aiData.Intern_SpawnPosY, startTime, startTime + 0.5f, sinusMove: false);
        }
    }

    public override bool Update(AIProcessData aiData, float clockTime)
    {
        _currentRatio = 0f;
        if (LookAround_LookTime != 0f)
        {
            _currentRatio = GetBehaviorRatio(aiData, clockTime);
        }
        aiData.SyncInit_ProgressRatio = _currentRatio;
        if (_currentRatio < 0.5f)
        {
            if (_currentStep != 1)
            {
                _currentStep = 1;
                aiData.Intern_AnimName = "IdleAlert";
            }
        }
        else if (_currentStep != 2)
        {
            _currentStep = 2;
            aiData.Intern_AnimName = "IdleAlert";
        }
        if (LookAround_SnapOnGround)
        {
            _goTo.Update(ref aiData, clockTime);
        }
        return true;
    }

    public override float GetBehaviorRatio(AIProcessData aiData, float clockTime)
    {
        if (clockTime > _behaviorStartTime + _behaviorDuration / aiData.Sync_SpeedFactor)
        {
            int num = (int)((clockTime - _behaviorStartTime) / (_behaviorDuration / aiData.Sync_SpeedFactor)) + 1;
            if (_behaviorStartTime + num * (_behaviorDuration / aiData.Sync_SpeedFactor) > clockTime)
            {
                num--;
            }
            _behaviorStartTime += num * (_behaviorDuration / aiData.Sync_SpeedFactor);
        }
        float num2 = (clockTime - _behaviorStartTime) / (_behaviorDuration / aiData.Sync_SpeedFactor);
        if (num2 > 1f)
        {
            num2 = 1f;
        }
        else if (num2 < 0f)
        {
            num2 = 0f;
        }
        return num2;
    }
}
