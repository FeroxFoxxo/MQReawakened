using System.Text.Json;

namespace Server.Base.Core.Extensions;

public static class DeepCopyClass
{
    public static T DeepCopy<T>(this T other)
    {
        var tmpStr = JsonSerializer.Serialize(other);
        return JsonSerializer.Deserialize<T>(tmpStr);
    }
}
