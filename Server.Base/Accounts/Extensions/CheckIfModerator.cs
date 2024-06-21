using Server.Base.Accounts.Enums;
using Server.Base.Database.Accounts;

namespace Server.Base.Accounts.Extensions;

public static class CheckIfModerator
{
    public static int IsModerator(this AccountModel account) => account.AccessLevel >= AccessLevel.Moderator ? 1 : 0;
}
