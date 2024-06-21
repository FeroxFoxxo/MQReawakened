using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Models;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;

namespace Server.Base.Database.Accounts;

public class AccountModel(AccountDbEntry entry) : INetStateData
{
    public int Id => entry.Id;
    public AccountDbEntry Write => entry;

    public string Username => entry.Username;
    public string Password => entry.Password;
    public string Email => entry.Email;
    public AccessLevel AccessLevel => entry.AccessLevel;
    public GameMode GameMode => entry.GameMode;
    public DateTime Created => entry.Created;
    public DateTime LastLogin => entry.LastLogin;
    public string[] IpRestrictions => entry.IpRestrictions;
    public string[] LoginIPs => entry.LoginIPs;
    public List<AccountTag> Tags => entry.Tags;
    public int Flags => entry.Flags;

    public void RemovedState(NetState state, IServiceProvider services, ILogger logger)
    {
        var nHandler = services.GetRequiredService<NetStateHandler>();
        logger.LogError("Disconnected. [{Count} Online] [{Username}]", nHandler.Instances.Count, Username);

        logger.LogTrace("Saving account data for {Username}", Username);
        var handler = services.GetRequiredService<AccountHandler>();
        handler.Update(Write);
    }
}
