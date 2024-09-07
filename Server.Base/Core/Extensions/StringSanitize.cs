namespace Server.Base.Core.Extensions;
public static class StringSanitize
{
    public static string Sanitize(this string str) => str?.Trim().ToLower();
}
