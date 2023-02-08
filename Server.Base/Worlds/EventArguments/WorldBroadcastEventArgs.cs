namespace Server.Base.Worlds.EventArguments;

public class WorldBroadcastEventArgs
{
    public string Message { get; }
    public WorldBroadcastEventArgs(string msg) => Message = msg;
}
