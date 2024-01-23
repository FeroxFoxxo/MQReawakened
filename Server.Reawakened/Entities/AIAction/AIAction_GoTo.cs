using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.Entities.AIAction;
internal class AIAction_GoTo
{
    private float _fromPosX;
    private float _fromPosY;
    private float _toPosX;
    private float _toPosY;
    private float _initialTime;
    private float _finalTime;
    private bool _sinusMove;

    public AIAction_GoTo(ref AIProcessData aiData, float fromPosX, float fromPosY, float toPosX, float toPosY, float initialTime, float finalTime, bool sinusMove)
    {
        _sinusMove = sinusMove;
        _initialTime = initialTime;
        _finalTime = finalTime;
        _toPosX = toPosX;
        _toPosY = toPosY;
        _fromPosX = fromPosX;
        _fromPosY = fromPosY;
        if (Math.Abs(_toPosX - _fromPosX) > 0f)
            aiData.SyncInit_Dir = Math.Sign(_toPosX - fromPosX);
    }

    public void Update(ref AIProcessData aiData, float clockTime)
    {
        var num = 1f;
        if (_finalTime != _initialTime)
            num = (clockTime - _initialTime) / (_finalTime - _initialTime);
        if (num > 1f)
            num = 1f;
        else if (num < 0f)
            num = 0f;
        if (_sinusMove)
            num = 0.5f * ((float)Math.Sin(num * 3.1416f - 1.5708f) + 1f);

        aiData.Sync_PosX = (1f - num) * _fromPosX + num * _toPosX;
        aiData.Sync_PosY = (1f - num) * _fromPosY + num * _toPosY;
        aiData.Intern_BehaviorRequestTime = num;
    }
}
