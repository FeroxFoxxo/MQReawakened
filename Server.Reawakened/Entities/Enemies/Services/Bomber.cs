namespace Server.Reawakened.Entities.Enemies.Services;
public class Bomber : IBomber
{
    public override void bomb(int direction) => LogFacade.error("Running unimplemented AI method 'bomb' (from Bomber.cs)");
}
