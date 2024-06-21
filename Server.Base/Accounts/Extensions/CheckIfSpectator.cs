using Server.Base.Accounts.Enums;
using Server.Base.Database.Accounts;

namespace Server.Base.Accounts.Extensions;

public static class CheckIfSpectator
{
    public static int IsSpectator(this AccountModel account) => account.GameMode == GameMode.Spectator ? 1 : 0;
}
