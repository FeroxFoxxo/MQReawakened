using System.Reflection;

namespace Server.Base.Core.Extensions;

public static class ReflectionExtensions
{
    public static void SetPrivateField<T>(this T instance, string fieldName, object fieldValue)
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var type = typeof(T);
        var field = type.GetField(fieldName, bindingFlags);

        if (field != null)
            field.SetValue(instance, fieldValue);
        else
            throw new MissingFieldException($"{type.Name} is missing field {fieldName}. " +
                                            $"Possible fields: {string.Join(", ", type.GetFields(bindingFlags).Select(x => x.Name))}");
    }

    public static MethodInfo GetPrivateMethod<T>(this T o, string methodName)
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var type = typeof(T);
        var mi = o.GetType().GetMethod(methodName, bindingFlags);

        return mi ?? throw new MissingMethodException($"{type.Name} is missing method {methodName}. " +
                                        $"Possible methods: {string.Join(", ", type.GetMethods(bindingFlags).Select(x => x.Name))}");
    }
}
