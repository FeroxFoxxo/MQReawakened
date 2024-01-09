using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Helpers;
using Server.Base.Accounts.Models;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Base.Network.Helpers;
using System.Net;

namespace Server.Base.Accounts.Services;

public class AccountHandler(EventSink sink, ILogger<Account> logger, InternalRConfig rConfig,
    PasswordHasher hasher, AccountAttackLimiter attackLimiter, IpLimiter ipLimiter,
    FileLogger fileLogger, InternalRwConfig rwConfig, TemporaryDataStorage temporaryDataStorage) :
    DataHandler<Account>(sink, logger, rConfig, rwConfig)
{
    public override bool HasDefault => true;

    public Dictionary<IPAddress, int> IpTable = [];

    public override void OnAfterLoad() => CreateIpTables();

    public override Account CreateDefault()
    {
        Logger.LogInformation("Username: ");
        var username = Console.ReadLine();

        Logger.LogInformation("Password: ");
        var password = Console.ReadLine();

        Logger.LogInformation("Email: ");
        var email = Console.ReadLine();

        if (username != null)
            return new Account(username, password, email, hasher)
            {
                AccessLevel = AccessLevel.Owner
            };

        Logger.LogError("Username for account is null!");
        return null;
    }

    public AlrReason GetAccount(string username, string password, NetState netState)
    {
        var rejectReason = AlrReason.Invalid;

        if (!RConfig.SocketBlock && !ipLimiter.Verify(netState.Address))
        {
            IpLimitedError(netState);
            rejectReason = AlrReason.InUse;
        }
        else
        {
            Account account;

            if (username == ".")
            {
                account = temporaryDataStorage.GetData<Account>(password);
                if (account == null)
                    rejectReason = AlrReason.BadComm;
                else
                    username = account.Username;
            }
            else
            {
                account = Data.Values.FirstOrDefault(a => a.Username == username);

                if (account != null)
                    if (!hasher.CheckPassword(account, password))
                        rejectReason = AlrReason.BadPass;
            }

            if (account != null)
            {
                if (!account.HasAccess(netState, this, RConfig))
                {
                    rejectReason = RConfig.LockDownLevel > AccessLevel.Vip
                        ? AlrReason.BadComm
                        : AlrReason.BadPass;
                }
                else if (account.IsBanned())
                {
                    rejectReason = AlrReason.Blocked;
                }
                else if (rejectReason is not AlrReason.BadPass and not AlrReason.BadComm)
                {
                    netState.Set(account);
                    rejectReason = AlrReason.Accepted;

                    account.LogAccess(netState, this, RConfig);
                }
            }
        }

        var errorReason = rejectReason switch
        {
            AlrReason.Accepted => "Valid credentials",
            AlrReason.BadComm => "Access denied",
            AlrReason.BadPass => "Invalid password",
            AlrReason.Blocked => "Banned account",
            AlrReason.InUse => "Past IP limit threshold",
            AlrReason.Invalid => "Invalid username",
            _ => throw new ArgumentOutOfRangeException(rejectReason.ToString())
        };

        fileLogger.WriteGenericLog<AccountHandler>("login", $"Login: {netState}", $"{errorReason} for '{username}'",
            rejectReason == AlrReason.Accepted ? LoggerType.Debug : LoggerType.Error);

        if (rejectReason is not AlrReason.Accepted and not AlrReason.InUse)
            attackLimiter.RegisterInvalidAccess(netState);

        return rejectReason;
    }

    public void CreateIpTables()
    {
        IpTable = [];

        foreach (var account in Data.Values.Where(account => account.LoginIPs.Length > 0))
        {
            if (IPAddress.TryParse(account.LoginIPs[0], out var ipAddress))
            {
                IpTable[ipAddress] = IpTable.TryGetValue(ipAddress, out var value) ? ++value : 1;
            }
            else
            {
                Logger.LogError("Unable to parse IPAddress {IP} for {Username}",
                    account.LoginIPs[0], account.Username);
            }
        }
    }

    public bool CanCreate(IPAddress ipAddress) => !IpTable.ContainsKey(ipAddress) || IpTable[ipAddress] < 1;

    public Account Create(IPAddress ipAddress, string username, string password, string email)
    {
        if (username.Trim().Length <= 0 || password.Trim().Length <= 0 || email.Trim().Length <= 0)
        {
            Logger.LogInformation("Login: {Address}: User post data for '{Username}' is invalid in length!",
                ipAddress, username);
            throw new InvalidOperationException();
        }

        var isSafe = !(username.StartsWith(' ') || username.EndsWith(' ') || username.EndsWith('.'));

        for (var i = 0; isSafe && i < username.Length; ++i)
        {
            isSafe = username[i] >= 0x20 && username[i] < 0x7F &&
                     RConfig.ForbiddenChars.All(t => username[i] != t);
        }

        for (var i = 0; isSafe && i < password.Length; ++i)
            isSafe = password[i] is >= (char)0x20 and < (char)0x7F;

        if (!isSafe)
        {
            Logger.LogInformation("Login: {Address}: User password for '{Username}' is unsafe! Returning...",
                ipAddress, username);
            return null;
        }

        if (!CanCreate(ipAddress))
        {
            Logger.LogWarning(
                "Login: {Address}: Account '{Username}' not created, ip already has {Accounts} account{Plural}.",
                ipAddress, username, RConfig.MaxAccountsPerIp,
                RConfig.MaxAccountsPerIp == 1 ? string.Empty : "s");
            return null;
        }

        Logger.LogInformation("Login: {Address}: Creating new account '{Username}'",
            ipAddress, username);

        var account = new Account(username, password, email, hasher);

        Add(account);

        return account;
    }

    public void IpLimitedError(NetState netState) =>
        fileLogger.WriteGenericLog<IpLimiter>("ipLimits", netState.ToString(), "Past IP limit threshold", LoggerType.Debug);
}
