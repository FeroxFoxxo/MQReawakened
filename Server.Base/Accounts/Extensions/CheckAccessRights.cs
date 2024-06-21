using Server.Base.Accounts.Enums;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Database.Accounts;
using Server.Base.Network;
using System.Net;

namespace Server.Base.Accounts.Extensions;

public static class CheckAccessRights
{
    public static bool HasAccess(this AccountModel account, NetState netState,
        InternalRConfig config) =>
        netState != null && account.HasAccess(netState.Address, config);

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
}
