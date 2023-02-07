using System.Reflection;

namespace Server.Base.Core.Extensions;

public static class ReflectionExtensions
{
    private const BindingFlags Bindings = BindingFlags.Public | BindingFlags.Static |
                                              BindingFlags.NonPublic | BindingFlags.Instance;

    public static void SetField<T>(this T instance, string fieldName, object fieldValue)
    {
        var type = typeof(T);
        var field = type.GetField(fieldName, Bindings);

        if (field != null)
            field.SetValue(instance, fieldValue);
        else
            throw new MissingFieldException($"{type.Name} is missing field {fieldName}. " +
                                            $"Possible fields: {string.Join(", ", type.GetFields(Bindings).Select(x => x.Name))}");
    }

    public static object GetField<T>(this T instance, string fieldName)
    {
        var type = typeof(T);
        var field = type.GetField(fieldName, Bindings);

        return field != null
            ? field.GetValue(instance)
            : throw new MissingFieldException($"{type.Name} is missing field {fieldName}. " +
                                              $"Possible fields: {string.Join(", ", type.GetFields(Bindings).Select(x => x.Name))}");
    }

    public static MethodInfo GetMethod<T>(this T o, string methodName)
    {
        var type = typeof(T);
        var mi = o.GetType().GetMethod(methodName, Bindings);

        return mi ?? throw new MissingMethodException($"{type.Name} is missing method {methodName}. " +
                                                      $"Possible methods: {string.Join(", ", type.GetMethods(Bindings).Select(x => x.Name))}");
    }
}
