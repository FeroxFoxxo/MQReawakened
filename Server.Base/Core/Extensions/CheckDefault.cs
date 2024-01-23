namespace Server.Base.Core.Extensions;

public static class CheckDefault
{
    public static bool IsDefault<T>(this T value) where T : struct => value.Equals(default(T));
}
