using Server.Base.Core.Abstractions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Server.Reawakened.Rooms.Services;

public class ClassCopier : IService
{
    private List<Type> _appliableAttributes;
    private Dictionary<Type, object> _instances;
    private Dictionary<Type, FieldInfo[]> _fields;
    private object _lock;

    public void Initialize()
    {
        _appliableAttributes = [
            typeof(MQAttribute),
            typeof(MQConstant),
            typeof(MQAttributeSerializePrefabValue),
            typeof(MQAttributeGlobalPerPrefab)
        ];

        _instances = [];
        _fields = [];
        _lock = new object();
    }

    public KeyValuePair<object, FieldInfo[]> GetClassAndInfo(Type mqType)
    {
        lock (_lock)
        {
            if (!_instances.ContainsKey(mqType))
                CreateClass(mqType);

            var originalObj = _instances[mqType];
            var fields = _fields[mqType];

            var copiedObj = RuntimeHelpers.GetUninitializedObject(mqType);

            foreach (var field in fields)
                field.SetValue(copiedObj, field.GetValue(originalObj));
            
            return new KeyValuePair<object, FieldInfo[]>(copiedObj, _fields[mqType]);
        }
    }

    private void CreateClass(Type mqType)
    {
        var dataObj = RuntimeHelpers.GetUninitializedObject(mqType);

        var ctor = mqType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, []);

        try
        {
            ctor.Invoke(dataObj, null);
        }
        catch (TargetInvocationException)
        { }

        var fields = mqType.GetFields()
            .Where(prop => _appliableAttributes.Any(a => prop.IsDefined(a, false)))
            .ToArray();

        _instances.Add(mqType, dataObj);
        _fields.Add(mqType, fields);
    }
}
