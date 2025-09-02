using Server.Base.Core.Extensions;
using Server.Base.Database.Accounts;
using System.Xml;

namespace Server.Base.Accounts.Extensions;
public static class CheckMutes
{
    public static bool IsMuted(this AccountModel account)
    {
        var isMuted = account.GetFlag(1);

        if (!isMuted)
            return false;

        if (!account.GetMuteTags(out var muteTime, out var muteDuration) || muteDuration == TimeSpan.MaxValue ||
            DateTime.UtcNow < muteTime + muteDuration)
            return true;

        account.SetUnspecifiedMute(null);
        account.SetMuted(false);
        return false;
    }

    public static void SetMuted(this AccountModel account, bool isMuted) =>
        account.SetFlag(1, isMuted);

    public static void SetMuteTags(this AccountModel account, string from, DateTime muteTime, TimeSpan muteDuration)
    {
        if (from == null)
            account.RemoveTag("MuteDealer");
        else
            account.SetTag("MuteDealer", from);

        if (muteTime == DateTime.MinValue)
            account.RemoveTag("MuteTime");
        else
            account.SetTag("MuteTime", XmlConvert.ToString(muteTime, XmlDateTimeSerializationMode.Utc));

        if (muteDuration == TimeSpan.Zero)
            account.RemoveTag("MuteDuration");
        else
            account.SetTag("MuteDuration", muteDuration.ToString());
    }

    public static void SetUnspecifiedMute(this AccountModel account, string from) =>
        account.SetMuteTags(from, DateTime.MinValue, TimeSpan.Zero);

    public static bool GetMuteTags(this AccountModel account, out DateTime muteTime, out TimeSpan muteDuration)
    {
        var tagTime = account.GetTag("MuteTime");
        var tagDuration = account.GetTag("MuteDuration");

        muteTime = tagTime != null ? XmlConverter.GetXmlDateTime(tagTime, DateTime.MinValue) : DateTime.MinValue;

        if (tagDuration == "Infinite")
            muteDuration = TimeSpan.MaxValue;
        else if (tagDuration != null)
            muteDuration = GetTime.ToTimeSpan(tagDuration);
        else
            muteDuration = TimeSpan.Zero;

        return muteTime != DateTime.MinValue && muteDuration != TimeSpan.Zero;
    }

    public static string FormatMuteTime(this AccountModel account)
    {
        var tagTime = account.GetTag("MuteTime");
        var tagDuration = account.GetTag("MuteDuration");

        var muteTime = tagTime != null ? XmlConverter.GetXmlDateTime(tagTime, DateTime.MinValue) : DateTime.MinValue;
        TimeSpan muteDuration;

        if (tagDuration == "Infinite")
            muteDuration = TimeSpan.MaxValue;
        else if (tagDuration != null)
            muteDuration = GetTime.ToTimeSpan(tagDuration);
        else
            muteDuration = TimeSpan.Zero;

        return account.FormatTime(muteTime, muteDuration);
    }
}
