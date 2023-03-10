namespace Server.Base.Core.Events.Arguments;

public class CrashedEventArgs : EventArgs
{
    public Exception Exception { get; }
    public bool Close { get; set; }
    public CrashedEventArgs(Exception ex) => Exception = ex;
}
