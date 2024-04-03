namespace Server.Base.Network.Events;

public class NetStateAddedEventArgs(NetState state)
{
    public NetState State => state;
}
