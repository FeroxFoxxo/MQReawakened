using System.Net.Sockets;

namespace Server.Base.Core.Events.Arguments;

public class SocketConnectEventArgs(Socket s) : EventArgs
{
    public Socket Socket => s;
    public bool AllowConnection { get; set; } = true;
}
