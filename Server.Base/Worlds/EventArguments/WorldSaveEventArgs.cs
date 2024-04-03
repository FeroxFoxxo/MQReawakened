namespace Server.Base.Worlds.EventArguments;

public class WorldSaveEventArgs(bool msg) : EventArgs
{
    public bool Message => msg;
}
