namespace Server.Reawakened.Entities.AIAction;
public class AIAction_Shoot
{
    private readonly int _nbBulletsPerRound;
    private readonly int _nbFireRounds;
    private readonly float _delayBetweenBullets;
    private readonly float _fireSpreadAngle;
    private readonly float _delayBetweenFireRound;
    private readonly float _projectileSpeed;
    private readonly float _delayShot_Anim;
    private readonly float _fireSpreadStartAngle;
    private readonly float _startTime;
    private readonly bool _fireSpreadClockwise;
    private readonly bool _fireLobTrajectory;

    private int _currentFireRound;
    private int _currentBulletInFireRound;

    public AIAction_Shoot(ref AIProcessData _, float startTime, int nbBulletsPerRound, float fireSpreadStartAngle, float fireSpreadAngle, int nbFireRounds, float delayBetweenBullets, float delayBetweenFireRound, float projectileSpeed, bool fireSpreadClockwise, float delayShot_Anim, bool lobTrajectory)
    {
        _currentFireRound = -1;
        _startTime = startTime;
        _fireSpreadAngle = fireSpreadAngle;
        _nbBulletsPerRound = nbBulletsPerRound;
        _nbFireRounds = nbFireRounds;
        _delayBetweenBullets = delayBetweenBullets;
        _delayBetweenFireRound = delayBetweenFireRound;
        _projectileSpeed = projectileSpeed;
        _fireSpreadClockwise = fireSpreadClockwise;
        _fireSpreadStartAngle = fireSpreadStartAngle;
        _delayShot_Anim = delayShot_Anim;
        _fireLobTrajectory = lobTrajectory;
    }

    public void Update(ref AIProcessData aiData, float clockTime)
    {
        var num = (int) ((clockTime - _startTime) / _delayBetweenFireRound);

        if (_nbFireRounds == 1)
            num = 0;

        var num2 = _startTime + num * _delayBetweenFireRound;

        if (num < _nbFireRounds)
        {
            if (_currentFireRound != num)
            {
                _currentFireRound = num;
                _currentBulletInFireRound = -1;
                aiData.Intern_AnimName = "Shoot";
                aiData.Intern_ForceAnimRepetition = true;
            }

            var num3 = 0;

            if (_delayBetweenBullets > 0f)
                num3 = (int)((clockTime - (num2 + _delayShot_Anim)) / _delayBetweenBullets);

            if (clockTime - (num2 + _delayShot_Anim) < 0f)
                num3 = -1;

            if (_currentBulletInFireRound != num3 && num3 < _nbBulletsPerRound)
            {
                _currentBulletInFireRound = num3;
                ShootProjectile(ref aiData);
            }
        }
    }

    private void ShootProjectile(ref AIProcessData aiData)
    {
        var num = _fireSpreadStartAngle;

        aiData.Intern_FireSpeed = _projectileSpeed;
        aiData.Intern_FireProjectile = true;
        aiData.Intern_FireAngle = 0f;
        aiData.Intern_FireLobTrajectory = _fireLobTrajectory;

        if (_nbBulletsPerRound > 1)
            aiData.Intern_FireAngle = _currentBulletInFireRound * _fireSpreadAngle / (_nbBulletsPerRound - 1);

        if (_fireSpreadClockwise && aiData.Intern_Dir == 1 || !_fireSpreadClockwise && aiData.Intern_Dir == -1)
            aiData.Intern_FireAngle = 0f - aiData.Intern_FireAngle;

        if (aiData.Intern_Dir == -1)
            num = 180f - _fireSpreadStartAngle;

        aiData.Intern_FireAngle = (aiData.Intern_FireAngle + num) * 0.01745f;
    }
}
