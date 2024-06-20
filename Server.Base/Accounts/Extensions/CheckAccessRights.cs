using Server.Base.Accounts.Database;
using Server.Base.Accounts.Enums;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using System.Net;

namespace Server.Base.Accounts.Extensions;

public static class CheckAccessRights
{
    public static bool HasAccess(this AccountModel account, NetState netState,
        InternalRConfig config) =>
        netState != null && account.HasAccess(netState.Address, config);

    public static bool CheckAccess(this AccountModel account, IPAddress ipAddress, AccountHandler handler,
        InternalRConfig config)
    {
        var hasAccess = account.HasAccess(ipAddress, config);

        if (hasAccess)
            account.LogAccess(ipAddress, handler);

        return hasAccess;
    }

    public static bool HasAccess(this AccountModel account, IPAddress ipAddress, InternalRConfig config)
    {
        var accessLevel = config.LockDownLevel;

        if (accessLevel >= AccessLevel.Moderator)
        {
            var hasAccess = account.AccessLevel >= accessLevel;

            if (!hasAccess)
                return false;
        }

        var accessAllowed = account.IpRestrictions.Length == 0;

        for (var i = 0; !accessAllowed && i < account.IpRestrictions.Length; ++i)
            accessAllowed = ipAddress.IpMatch(account.IpRestrictions[i]);

        return accessAllowed;
    }

    public static void LogAccess(this AccountModel account, NetState netState, AccountHandler handler)
    {
        if (netState != null)
            account.LogAccess(netState.Address, handler);
    }

    public static void LogAccess(this AccountModel account, IPAddress ipAddress, AccountHandler handler)
    {
        if (account.LoginIPs.Length == 0)
            handler.IpTable[ipAddress] = handler.IpTable.TryGetValue(ipAddress, out var value) ? value + 1 : 1;

        var contains = false;

        for (var i = 0; !contains && i < account.LoginIPs.Length; ++i)
            contains = account.LoginIPs[i].Equals(ipAddress.ToString());

        if (contains)
            return;

        var oldIPs = account.LoginIPs;

        account.Write.LoginIPs = new string[oldIPs.Length + 1];

        for (var i = 0; i < oldIPs.Length; ++i)
            account.LoginIPs[i] = oldIPs[i];

        account.LoginIPs[oldIPs.Length] = ipAddress.ToString();
    }
}
