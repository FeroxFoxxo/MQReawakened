using Server.Reawakened.Entities.AIAction;
using UnityEngine;

namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Patrol : AIBaseBehavior
{
    private Vector3 _spawnPosition;
    private Vector3 _destinationPosition;
    private float _currentRatio;
    private float _behaviorStartTime;
    private readonly float _behaviorDuration;
    private int _currentStep;
    private readonly float _firstStepRatio;
    private readonly float _secondStepRatio;
    private readonly float _thirdStepRatio;
    public float Patrol_MoveSpeed;
    public float Patrol_EndPathWaitTime;
    private AIAction_GoTo _action;

    public AIBehavior_Patrol(Vector3 spawnPosition, Vector3 destinationPosition, float patrolSpeed, float endPathWaitTime)
    {
        _spawnPosition = spawnPosition;
        _destinationPosition = destinationPosition;

        Patrol_MoveSpeed = patrolSpeed;
        Patrol_EndPathWaitTime = endPathWaitTime;

        var _distanceLength = (float)Math.Sqrt((destinationPosition.x - spawnPosition.x) * (destinationPosition.x - spawnPosition.x) + (destinationPosition.y - spawnPosition.y) * (destinationPosition.y - spawnPosition.y));

        _behaviorDuration = 2f * (_distanceLength / Patrol_MoveSpeed + Patrol_EndPathWaitTime);
        _firstStepRatio = _distanceLength / Patrol_MoveSpeed / _behaviorDuration;
        _secondStepRatio = _firstStepRatio + Patrol_EndPathWaitTime / _behaviorDuration;
        _thirdStepRatio = _secondStepRatio + _distanceLength / Patrol_MoveSpeed / _behaviorDuration;
        _currentStep = 0;
    }

    public override bool Update(AIProcessData aiData, float roomTime)
    {
        roomTime -= Patrol_EndPathWaitTime;
        _currentRatio = GetBehaviorRatio(aiData, roomTime);
        aiData.SyncInit_ProgressRatio = _currentRatio;

        if (_currentRatio < _firstStepRatio)
        {
            if (_currentStep != 1)
            {
                _currentStep = 1;

                var behaviorStartTime = _behaviorStartTime;
                var finalTime = behaviorStartTime + _firstStepRatio * _behaviorDuration;

                _action = new AIAction_GoTo(ref aiData, _spawnPosition.x, _spawnPosition.y, _destinationPosition.x, _destinationPosition.y, behaviorStartTime, finalTime, sinusMove: false);
            }
            _action.Update(ref aiData, roomTime);
        }
        else if (_currentRatio < _secondStepRatio && Patrol_EndPathWaitTime > 0f)
        {
            if (_currentStep != 2)
                _currentStep = 2;
            
            aiData.Sync_PosX = _destinationPosition.x;
            aiData.Sync_PosY = _destinationPosition.y;
        }
        else if (_currentRatio < _thirdStepRatio)
        {
            if (_currentStep != 3)
            {
                _currentStep = 3;

                var num = _behaviorStartTime + _secondStepRatio * _behaviorDuration;
                var finalTime2 = num + (_thirdStepRatio - _secondStepRatio) * _behaviorDuration;

                _action = new AIAction_GoTo(ref aiData, _destinationPosition.x, _destinationPosition.y, _spawnPosition.x, _spawnPosition.y, num, finalTime2, sinusMove: false);
            }
            _action.Update(ref aiData, roomTime);
        }
        else if (Patrol_EndPathWaitTime > 0f)
        {
            if (_currentStep != 4)
                _currentStep = 4;

            aiData.Sync_PosX = _spawnPosition.x;
            aiData.Sync_PosY = _spawnPosition.y;
        }
        return true;
    }

    public override float GetBehaviorRatio(AIProcessData aiData, float roomTime)
    {
        if (roomTime > _behaviorStartTime + _behaviorDuration)
        {
            var num = (int)((roomTime - _behaviorStartTime) / _behaviorDuration);

            if (_behaviorStartTime + num * _behaviorDuration > roomTime)
                num--;

            _behaviorStartTime += num * _behaviorDuration;
        }

        var num2 = (roomTime - _behaviorStartTime) / _behaviorDuration;

        if (num2 > 1f)
            num2 = 1f;
        else if (num2 < 0f)
            num2 = 0f;

        return num2;
    }
}
