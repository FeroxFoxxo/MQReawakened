namespace Server.Reawakened.Entities.Enemies.Services;
public class Shooter : IShoot
{
    public override int projectile(float clockTime, float speedX, float speedY, float posX, float posY, float posZ, bool isLob)
    {
        LogFacade.error("Running unimplemented AI method 'projectile (x,y,z)' (from Shooter.cs)");
        return 0;
    }

    public override int projectile(float clockTime, float speed, float maxHeight, vector3 pos, vector3 target)
    {
        LogFacade.error("Running unimplemented AI method 'projectile (vector3)' (from Shooter.cs)");
        return 0;
    }

    public override void spread(float clockTime, float speed, int count, float spreadAngle, vector3 origin, vector3 target) =>
        LogFacade.error("Running unimplemented AI method 'spread' (from Shooter.cs)");
}
