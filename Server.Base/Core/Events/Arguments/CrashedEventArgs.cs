namespace Server.Base.Core.Events.Arguments;

public class CrashedEventArgs(Exception ex) : EventArgs
{
    public Exception Exception => ex;
    public bool Close { get; set; }
}
