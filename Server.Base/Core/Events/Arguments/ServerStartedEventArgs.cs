using Server.Base.Core.Abstractions;

namespace Server.Base.Core.Events.Arguments;

public class ServerStartedEventArgs(IEnumerable<Module> modules) : EventArgs
{
    public IEnumerable<Module> Modules => modules;
}
