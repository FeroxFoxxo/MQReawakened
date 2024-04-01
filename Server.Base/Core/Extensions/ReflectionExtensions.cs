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

    public static void SetPropertyType<T>(this T instance, string propertyType, object propertyValue)
    {
        var type = instance.GetType();
        var property = type.GetProperty(propertyType, Bindings);

        if (property != null)
            property.SetValue(instance, propertyValue);
        else
            throw new MissingFieldException($"{type.Name} is missing property {propertyType}. " +
                                              $"Possible properties: {string.Join(", ", type.GetProperties(Bindings).Select(x => x.Name))}");
    }

    public static Type GetPropertyType<T>(this T instance, string propertyName)
    {
        var type = instance.GetType();
        var property = type.GetProperty(propertyName, Bindings);

        return property != null
            ? property.PropertyType
            : throw new MissingFieldException($"{type.Name} is missing property {propertyName}. " +
                                              $"Possible properties: {string.Join(", ", type.GetProperties(Bindings).Select(x => x.Name))}");
    }

    public static PropertyInfo[] GetProperties<T>(this T instance)
    {
        var type = instance.GetType();
        return type.GetProperties(Bindings);
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

    public static MethodInfo GetMethod<T>(this T instance, string methodName)
    {
        var type = instance.GetType();
        var method = type.GetMethod(methodName, Bindings);

        return method ?? throw new MissingMethodException($"{type.Name} is missing method {methodName}. " +
                                                      $"Possible methods: {string.Join(", ", type.GetMethods(Bindings).Select(x => x.Name))}");
    }
}
