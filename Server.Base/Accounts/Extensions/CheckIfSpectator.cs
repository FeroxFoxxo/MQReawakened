using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Models;

namespace Server.Base.Accounts.Extensions;

public static class CheckIfSpectator
{
    public static int IsSpectator(this Account account) => account.GameMode == GameMode.Spectator ? 1 : 0;
}
