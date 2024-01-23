﻿using Server.Reawakened.Entities.AIAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Server.Reawakened.Entities.AIBehavior;
public class AIBehavior_Aggro : AIBaseBehavior
{
    private float Aggro_AttackSpeed;
    private float Aggro_MoveBeyondTargetDistance;
    private bool Aggro_StayOnPatrolPath;
    private float Aggro_AttackBeyondPatrolLine;
    private float detectionHeightUp;
    private float detectionHeightDown;
    private float _fromPosX;
    private float _fromPosY;
    private float _toPosX;
    private float _toPosY;
    private int _currentStep;
    private float _behaviorDuration;
    private float _behaviorStartTime;
    private float _currentRatio;
    private AIAction_GoTo _goTo;

    public AIBehavior_Aggro(float attackSpeed, float moveBeyondTargetDistance, bool stayOnPatrolPath, float attackBeyondPatrolLine, float detectionHeightUp, float detectionHeightDown)
    {
        Aggro_AttackSpeed = attackSpeed;
        Aggro_MoveBeyondTargetDistance = moveBeyondTargetDistance;
        Aggro_StayOnPatrolPath = stayOnPatrolPath;
        Aggro_AttackBeyondPatrolLine = attackBeyondPatrolLine;
        this.detectionHeightUp = detectionHeightUp;
        this.detectionHeightDown = detectionHeightDown;
    }

    public override void Start(AIProcessData aiData, float startTime, string[] args)
    {
        _currentStep = 0;
        _currentRatio = 0f;
        _behaviorStartTime = startTime;
        _toPosX = aiData.Sync_TargetPosX;
        _toPosY = aiData.Sync_TargetPosY;
        _fromPosX = aiData.Sync_PosX;
        _fromPosY = aiData.Sync_PosY;
        if (Aggro_MoveBeyondTargetDistance > 0f)
        {
            float num = _toPosX - aiData.Sync_PosX;
            float num2 = _toPosY - aiData.Sync_PosY;
            float num3 = (float)Math.Sqrt(num * num + num2 * num2);
            if (num3 != 0f)
            {
                num /= num3;
                num2 /= num3;
            }
            _toPosX += num * Aggro_MoveBeyondTargetDistance;
            _toPosY += num2 * Aggro_MoveBeyondTargetDistance;
        }
        if (detectionHeightUp < 0.25f)
        {
            detectionHeightUp = 0.25f;
        }
        if (detectionHeightDown < 0.25f)
        {
            detectionHeightDown = 0.25f;
        }
        aiData.Utilities.ClampPointOnRectangle(ref _toPosX, ref _toPosY, _toPosX, _toPosY, aiData.Sync_PosX, aiData.Sync_PosY, aiData.Intern_MinPointX - Aggro_AttackBeyondPatrolLine, aiData.Intern_MaxPointX + Aggro_AttackBeyondPatrolLine, aiData.Intern_MinPointY - detectionHeightDown, aiData.Intern_MaxPointY + detectionHeightUp);
        if (Aggro_StayOnPatrolPath)
        {
            _toPosY = aiData.Intern_SpawnPosY;
        }
        float num4 = (float)Math.Sqrt((_toPosX - _fromPosX) * (_toPosX - _fromPosX) + (_toPosY - _fromPosY) * (_toPosY - _fromPosY));
        _behaviorDuration = num4 / Aggro_AttackSpeed;
    }

    public override bool Update(AIProcessData aiData, float clockTime)
    {
        if (_currentRatio == 1f)
        {
            return false;
        }
        _currentRatio = GetBehaviorRatio(aiData, clockTime);
        if (_currentStep != 1)
        {
            _currentStep = 1;
            float behaviorStartTime = _behaviorStartTime;
            float finalTime = behaviorStartTime + _behaviorDuration / aiData.Sync_SpeedFactor;
            _goTo = new AIAction_GoTo(ref aiData, _fromPosX, _fromPosY, _toPosX, _toPosY, behaviorStartTime, finalTime, sinusMove: false);
        }
        _goTo.Update(ref aiData, clockTime);
        if (_currentRatio == 1f)
        {
            return false;
        }
        return true;
    }

    public override float GetBehaviorRatio(AIProcessData aiData, float clockTime)
    {
        float num = (clockTime - _behaviorStartTime) / (_behaviorDuration / aiData.Sync_SpeedFactor);
        if (num > 1f)
        {
            num = 1f;
        }
        else if (num < 0f)
        {
            num = 0f;
        }
        return num;
    }
}
