using Server.Base.Accounts.Database;
using Server.Base.Accounts.Enums;

namespace Server.Base.Accounts.Extensions;

public static class CheckIfModerator
{
    public static int IsModerator(this AccountModel account) => account.AccessLevel >= AccessLevel.Moderator ? 1 : 0;
}
