using Newtonsoft.Json;

namespace Server.Base.Core.Extensions;

public static class DeepCopyClass
{
    public static T DeepCopy<T>(this T other)
    {
        var tmpStr = JsonConvert.SerializeObject(other);
        return JsonConvert.DeserializeObject<T>(tmpStr);
    }
}
