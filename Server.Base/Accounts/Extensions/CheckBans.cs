using Server.Base.Core.Extensions;
using Server.Base.Database.Accounts;
using System.Text.RegularExpressions;
using System.Xml;

namespace Server.Base.Accounts.Extensions;

public static class CheckBans
{
    public static bool IsBanned(this AccountModel account)
    {
        var isBanned = account.GetFlag(0);

        if (!isBanned)
            return false;

        if (!account.GetBanTags(out var banTime, out var banDuration) || banDuration == TimeSpan.MaxValue ||
            DateTime.UtcNow < banTime + banDuration)
            return true;

        account.SetUnspecifiedBan(null);
        account.SetBanned(false);
        return false;
    }

    public static void SetBanned(this AccountModel account, bool isBanned) =>
        account.SetFlag(0, isBanned);

    public static void SetBanTags(this AccountModel account, string from, DateTime banTime, TimeSpan banDuration)
    {
        if (from == null)
            account.RemoveTag("BanDealer");
        else
            account.SetTag("BanDealer", from);

        if (banTime == DateTime.MinValue)
            account.RemoveTag("BanTime");
        else
            account.SetTag("BanTime", XmlConvert.ToString(banTime, XmlDateTimeSerializationMode.Utc));

        if (banDuration == TimeSpan.Zero)
            account.RemoveTag("BanDuration");
        else
            account.SetTag("BanDuration", banDuration.ToString());
    }

    public static void SetUnspecifiedBan(this AccountModel account, string from) =>
        account.SetBanTags(from, DateTime.MinValue, TimeSpan.Zero);

    public static bool GetBanTags(this AccountModel account, out DateTime banTime, out TimeSpan banDuration)
    {
        var tagTime = account.GetTag("BanTime");
        var tagDuration = account.GetTag("BanDuration");

        banTime = tagTime != null ? XmlConverter.GetXmlDateTime(tagTime, DateTime.MinValue) : DateTime.MinValue;

        if (tagDuration == "Infinite")
            banDuration = TimeSpan.MaxValue;
        else if (tagDuration != null)
            banDuration = GetTime.ToTimeSpan(tagDuration);
        else
            banDuration = TimeSpan.Zero;

        return banTime != DateTime.MinValue && banDuration != TimeSpan.Zero;
    }

    public static TimeSpan ParseTime(this AccountModel _, string input)
    {
        var m = Regex.Match(input, @"^((?<days>\d+)d)?((?<hours>\d+)h)?((?<minutes>\d+)m)?((?<seconds>\d+)s)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);

        var ds = m.Groups["days"].Success ? int.Parse(m.Groups["days"].Value) : 0;
        var hs = m.Groups["hours"].Success ? int.Parse(m.Groups["hours"].Value) : 0;
        var ms = m.Groups["minutes"].Success ? int.Parse(m.Groups["minutes"].Value) : 0;
        var ss = m.Groups["seconds"].Success ? int.Parse(m.Groups["seconds"].Value) : 0;

        return TimeSpan.FromSeconds(ds * 60 * 60 * 24 + hs * 60 * 60 + ms * 60 + ss);
    }

    public static string FormatBanTime(this AccountModel account)
    {
        var tagTime = account.GetTag("BanTime");
        var tagDuration = account.GetTag("BanDuration");

        var banTime = tagTime != null ? XmlConverter.GetXmlDateTime(tagTime, DateTime.MinValue) : DateTime.MinValue;
        TimeSpan banDuration;

        if (tagDuration == "Infinite")
            banDuration = TimeSpan.MaxValue;
        else if (tagDuration != null)
            banDuration = GetTime.ToTimeSpan(tagDuration);
        else
            banDuration = TimeSpan.Zero;

        return account.FormatTime(banTime, banDuration);
    }

    public static string FormatTime(this AccountModel _, DateTime time, TimeSpan duration)
    {
        if (duration == TimeSpan.MaxValue)
            return " permanently";

        var elapsed = DateTime.UtcNow - time;
        var input = duration - elapsed;

        var output = string.Empty;

        if ((int)input.TotalDays > 0)
            output += " " + string.Format("{0}d", (int)input.TotalDays);

        if ((int)input.TotalHours > 0)
            output += " " + string.Format("{0}hr" +
                (input.Hours == 1 ? string.Empty : "s"), input.Hours);

        if (input.Minutes > 0)
            output += " " + string.Format("{0}min" +
                (input.Minutes == 1 ? string.Empty : "s"), input.Minutes);

        return " for" + output;
    }
}
