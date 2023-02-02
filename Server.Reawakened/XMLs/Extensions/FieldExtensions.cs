using System.Reflection;

namespace Server.Reawakened.XMLs.Extensions;

public static class FieldExtensions
{
    public static void SetPrivateField<T>(this T instance, string fieldName, object fieldValue)
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var type = typeof(T);
        var field = type.GetField(fieldName, bindingFlags );

        if (field != null)
            field.SetValue(instance, fieldValue);
        else
            throw new MissingFieldException($"{type.Name} is missing field {fieldName}. " +
                                            $"Possible fields: {string.Join(", ", type.GetFields(bindingFlags ).Select(x => x.Name))}");
    }
}
