using Server.Base.Accounts.Database;
using Server.Base.Accounts.Enums;

namespace Server.Base.Accounts.Extensions;

public static class CheckIfSpectator
{
    public static int IsSpectator(this AccountModel account) => account.GameMode == GameMode.Spectator ? 1 : 0;
}
