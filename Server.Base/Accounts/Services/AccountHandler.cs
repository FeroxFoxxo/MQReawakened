using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Helpers;
using Server.Base.Accounts.Models;
using Server.Base.Core.Events;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Base.Network.Helpers;
using System.Net;

namespace Server.Base.Accounts.Services;

public class AccountHandler : DataHandler<Account>
{
    private readonly AccountAttackLimiter _attackLimiter;
    private readonly InternalStaticConfig _config;
    private readonly PasswordHasher _hasher;
    private readonly InternalStaticConfig _internalServerConfig;
    private readonly IpLimiter _ipLimiter;
    private readonly FileLogger _fileLogger;
    private readonly TemporaryDataStorage _temporaryDataStorage;

    public Dictionary<IPAddress, int> IpTable;

    public AccountHandler(EventSink sink, ILogger<Account> logger, InternalStaticConfig internalServerConfig,
        PasswordHasher hasher, AccountAttackLimiter attackLimiter, IpLimiter ipLimiter,
        FileLogger fileLogger, InternalStaticConfig config, TemporaryDataStorage temporaryDataStorage) : base(
        sink, logger)
    {
        _internalServerConfig = internalServerConfig;
        _hasher = hasher;
        _attackLimiter = attackLimiter;
        _ipLimiter = ipLimiter;
        _fileLogger = fileLogger;
        _config = config;
        _temporaryDataStorage = temporaryDataStorage;
        IpTable = new Dictionary<IPAddress, int>();
    }

    public override void OnAfterLoad() => CreateIpTables();

    public override Account CreateDefault()
    {
        Logger.LogInformation("Username: ");
        var username = Console.ReadLine();

        Logger.LogInformation("Password: ");
        var password = Console.ReadLine();

        if (username != null)
            return new Account(username, password, Data.Count, _hasher)
            {
                AccessLevel = AccessLevel.Owner
            };

        Logger.LogError("Username for account is null!");
        return null;
    }

    public AlrReason GetAccount(string username, string password, NetState netState)
    {
        var rejectReason = AlrReason.Invalid;

        if (!_internalServerConfig.SocketBlock && !_ipLimiter.Verify(netState.Address))
        {
            IpLimitedError(netState);
            rejectReason = AlrReason.InUse;
        }
        else
        {
            Account account;

            if (username == ".")
            {
                account = _temporaryDataStorage.GetData<Account>(password);
                if (account == null)
                    rejectReason = AlrReason.BadComm;
                else
                    username = account.Username;
            }
            else
            {
                account = Data.Values.FirstOrDefault(a => a.Username == username);

                if (account != null)
                    if (!_hasher.CheckPassword(account, password))
                        rejectReason = AlrReason.BadPass;
            }

            if (account != null)
            {
                if (!account.HasAccess(netState, this, _config))
                {
                    rejectReason = _internalServerConfig.LockDownLevel > AccessLevel.Vip
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

                    account.LogAccess(netState, this, _config);
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

        if (rejectReason == AlrReason.Accepted)
            Logger.LogInformation("Login: {NetState}: {Reason} for '{Username}'", netState, errorReason, username);
        else
            Logger.LogError("Login: {NetState}: {Reason} for '{Username}'", netState, errorReason, username);

        if (rejectReason is not AlrReason.Accepted and not AlrReason.InUse)
            _attackLimiter.RegisterInvalidAccess(netState);

        return rejectReason;
    }

    public void CreateIpTables()
    {
        IpTable = new Dictionary<IPAddress, int>();

        foreach (var account in Data.Values.Where(account => account.LoginIPs.Length > 0))
        {
            if (IPAddress.TryParse(account.LoginIPs[0], out var ipAddress))
            {
                if (IpTable.TryGetValue(ipAddress, out var value))
                    value++;
                else
                    IpTable[ipAddress] = 1;
            }
            else
            {
                Logger.LogError("Unable to parse IPAddress {IP} for {Username}",
                    account.LoginIPs[0], account.Username);
            }
        }
    }

    public bool CanCreate(IPAddress ipAddress) => !IpTable.ContainsKey(ipAddress) || IpTable[ipAddress] < 1;

    public override Account Create(NetState netState, params string[] obj)
    {
        var username = obj[0] ?? string.Empty;
        var password = obj[1] ?? string.Empty;

        if (username.Trim().Length <= 0 || password.Trim().Length <= 0)
            throw new InvalidOperationException();

        if (username.Length == 0)
            return null;

        var isSafe = !(username.StartsWith(" ") || username.EndsWith(" ") || username.EndsWith("."));

        for (var i = 0; isSafe && i < username.Length; ++i)
        {
            isSafe = username[i] >= 0x20 && username[i] < 0x7F &&
                     _internalServerConfig.ForbiddenChars.All(t => username[i] != t);
        }

        for (var i = 0; isSafe && i < password.Length; ++i)
            isSafe = password[i] is >= (char)0x20 and < (char)0x7F;

        if (!isSafe)
            return null;

        if (!CanCreate(netState.Address))
        {
            Logger.LogWarning(
                "Login: {NetState}: Account '{Username}' not created, ip already has {Accounts} account{Plural}.",
                netState, username, _internalServerConfig.MaxAccountsPerIp,
                _internalServerConfig.MaxAccountsPerIp == 1 ? string.Empty : "s");
            return null;
        }

        Logger.LogInformation("Login: {NetState}: Creating new account '{Username}'",
            netState, username);

        return new Account(username, password, Data.Count, _hasher);
    }

    public void IpLimitedError(NetState netState) =>
        _fileLogger.WriteNetStateLog<IpLimiter>("ipLimits", netState, "Past IP limit threshold", LoggerType.Debug);
}
