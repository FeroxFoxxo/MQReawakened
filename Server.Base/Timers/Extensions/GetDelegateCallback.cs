namespace Server.Base.Timers.Extensions;

public static class GetDelegateCallback
{
    public static string FormatDelegate(this Delegate callback)
    {
        return callback == null
            ? "null"
            : callback.Method.DeclaringType == null
            ? callback.Method.Name
            : $"{callback.Method.DeclaringType.FullName}.{callback.Method.Name}";
    }
}
