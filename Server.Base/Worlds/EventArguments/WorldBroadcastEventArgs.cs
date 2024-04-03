namespace Server.Base.Worlds.EventArguments;

public class WorldBroadcastEventArgs(string msg)
{
    public string Message => msg;
}
