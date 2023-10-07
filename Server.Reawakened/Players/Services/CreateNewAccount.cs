using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;

namespace Server.Reawakened.Players.Services;

public class CreateNewAccount(ServerConsole serverConsole,
    EventSink eventSink) : IService
{
    public void Initialize() =>
        serverConsole.AddCommand(
            "createAccount",
            "Adds a new account to the server.",
            NetworkType.Server,
            AddAccount
        );

    private void AddAccount(string[] command) =>
        eventSink.InvokeCreateData();
}
