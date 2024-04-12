namespace Server.Base.Network.Events;

public class NetStateRemovedEventArgs(NetState state)
{
    public NetState State => state;
}
