using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;

namespace Server.Reawakened.Players.Services;

public class CreateNewAccount(ServerConsole serverConsole,
    EventSink eventSink) : IService
{
    private readonly EventSink _eventSink = eventSink;
    private readonly ServerConsole _serverConsole = serverConsole;

    public void Initialize() =>
        _serverConsole.AddCommand(
            "createAccount",
            "Adds a new account to the server.",
            NetworkType.Server,
            AddAccount
        );

    private void AddAccount(string[] command) =>
        _eventSink.InvokeCreateData();
}
