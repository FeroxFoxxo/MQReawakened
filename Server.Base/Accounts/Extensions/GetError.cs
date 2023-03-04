using Server.Base.Accounts.Enums;

namespace Server.Base.Accounts.Extensions;

public static class GetError
{
    public static string GetErrorValue(this AlrReason rejectReason) =>
        rejectReason switch
        {
            AlrReason.Invalid => "An incorrect username has been entered",
            AlrReason.BadComm => "Access from this account has been denied",
            AlrReason.BadPass => "You have entered an invalid password",
            AlrReason.Blocked => "Your account has been banned",
            AlrReason.InUse => "We have reached our thresh-hold for users",
            AlrReason.PlayerLoggedIn => "You are currently logged into the server",
            _ => "You have reached an error that should not have happened"
        };
}
