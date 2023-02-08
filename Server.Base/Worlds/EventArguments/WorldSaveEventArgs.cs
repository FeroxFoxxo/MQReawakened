namespace Server.Base.Worlds.EventArguments;

public class WorldSaveEventArgs : EventArgs
{
    public bool Message { get; }
    public WorldSaveEventArgs(bool msg) => Message = msg;
}
