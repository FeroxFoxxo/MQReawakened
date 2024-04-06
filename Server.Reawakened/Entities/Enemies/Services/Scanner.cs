namespace Server.Reawakened.Entities.Enemies.Services;
public class Scanner : IScan
{
    public override vector3 findTarget(float clockTime)
    {
        LogFacade.error("Running unimplemented AI method 'findTarget' (from Scanner.cs)");
        return new vector3(0, 0, 0);
    }

    public override vector3 findClosestTarget(float radius)
    {
        LogFacade.error("Running unimplemented AI method 'findClosestTarget' (from Scanner.cs)");
        return new vector3(0, 0, 0);
    }
}
