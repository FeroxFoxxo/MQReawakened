using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Logging;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.Json;
using UnityEngine;

namespace Server.Reawakened.Rooms.Services;

public class EntityComponentBuilder(ReflectionUtils reflectionUtils,
    ClassCopier classCopier,
    FileLogger fileLogger,
    AssetBundleRConfig assetBundleRConfig,
    IServiceProvider services) : IService
{
    public void Initialize() { }

    public List<BaseComponent> Build(GameObjectModel entity, Room room, out List<string> unknownComponents)
    {
        var components = new List<BaseComponent>();
        unknownComponents = [];

        var entityData = new Entity(entity, room, fileLogger);
        var prefabOverrides = room.World.GetPrefabOverloads(assetBundleRConfig, entity.ObjectInfo.PrefabName);

        EnsurePrefabComponentsPresent(entity, prefabOverrides);

        foreach (var component in entity.ObjectInfo.Components)
        {
            if (!room.World.ProcessableComponents.TryGetValue(component.Key, out var mqType))
            {
                if (!room.LoggedComponentKeys.Contains(component.Key))
                {
                    room.Logger.LogTrace("Could not find processable type for {ComponentKey}", component.Key);
                    room.LoggedComponentKeys.Add(component.Key);
                }
                continue;
            }

            if (!room.World.EntityComponents.TryGetValue(component.Key, out var internalType))
            {
                if (!room.LoggedComponentKeys.Contains(component.Key))
                {
                    room.Logger.LogTrace("Could not find internal type for {ComponentKey}", component.Key);
                    room.LoggedComponentKeys.Add(component.Key);
                }
                unknownComponents.Add(mqType.Name);
                continue;
            }

            var entityInfo = classCopier.GetClassAndInfo(mqType);
            var dataObj = entityInfo.Key;
            var fields = entityInfo.Value;

            if (prefabOverrides != null)
            {
                if (!prefabOverrides.TryGetValue(component.Key, out var value))
                    prefabOverrides.TryGetValue(mqType.Name, out value);

                if (value != null)
                    ApplyPrefabOverrides(value, dataObj, room.Logger);
            }

            ApplyXMLOverrides(component.Value.ComponentAttributes, dataObj, fields, room.Logger);

            var instancedComponent = reflectionUtils.CreateBuilder<BaseComponent>(internalType.GetTypeInfo())
                .Invoke(services);

            var methods = internalType.GetMethods().Where(m =>
            {
                var parameters = m.GetParameters();

                return
                    m.Name == "SetComponentData" &&
                    parameters.Length == 2 &&
                    parameters[0].ParameterType == dataObj.GetType() &&
                    parameters[1].ParameterType == entityData.GetType();
            }).ToArray();

            if (methods.Length != 1)
            {
                room.Logger.LogError(
                    "Found invalid {Count} amount of initialization methods for {EntityId} ({EntityType})",
                    methods.Length, entity, internalType.Name);
                continue;
            }

            methods.First().Invoke(instancedComponent, [dataObj, entityData]);
            components.Add(instancedComponent);
        }

        return components;
    }

    private static void EnsurePrefabComponentsPresent(GameObjectModel entity, Dictionary<string, OrderedDictionary> prefabOverrides)
    {
        if (prefabOverrides == null || prefabOverrides.Count == 0)
            return;

        foreach (var entry in prefabOverrides)
        {
            if (!entity.ObjectInfo.Components.ContainsKey(entry.Key))
                entity.ObjectInfo.Components.Add(entry.Key, new ComponentModel { ComponentAttributes = [] });
        }
    }

    private static void ApplyPrefabOverrides(OrderedDictionary prefabOverrides, object dataObj, Microsoft.Extensions.Logging.ILogger logger)
    {
        var fields = dataObj.GetType().GetFields();

        if (prefabOverrides.Count > 0)
        {
            foreach (var entry in prefabOverrides.Cast<DictionaryEntry>())
            {
                if (entry.Value == null)
                    continue;

                if (entry.Key is not string key)
                {
                    logger.LogError("Prefab override key is not a string: {Key} ({Type})", entry.Key, entry.Key.GetType());
                    continue;
                }

                var field = fields.FirstOrDefault(f => f.Name == key);

                if (field == null)
                {
                    logger.LogError("Prefab value: {Value} could not be found for component {Component}", entry.Key, dataObj.GetType().Name);
                    continue;
                }

                if (entry.Value is JsonElement element)
                {
                    switch (element.ValueKind)
                    {
                        case JsonValueKind.String:
                            if (field.FieldType == typeof(string))
                                field.SetValue(dataObj, element.GetString());
                            else
                                logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                            break;
                        case JsonValueKind.Number:
                            if (field.FieldType == typeof(int))
                                field.SetValue(dataObj, element.GetInt32());
                            else if (field.FieldType == typeof(float))
                                field.SetValue(dataObj, element.GetSingle());
                            else if (field.FieldType == typeof(bool))
                                field.SetValue(dataObj, element.GetInt32() == 1);
                            else if (field.FieldType.IsEnum)
                                field.SetValue(dataObj, Enum.ToObject(field.FieldType, element.GetInt32()));
                            else
                                logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                            break;
                        case JsonValueKind.False:
                        case JsonValueKind.True:
                            if (field.FieldType == typeof(bool))
                                field.SetValue(dataObj, element.GetBoolean());
                            else
                                logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                            break;
                        case JsonValueKind.Null:
                            break;
                        case JsonValueKind.Object:
                            switch (field.FieldType)
                            {
                                case var t when t == typeof(Vector2):
                                    var v2Props = element.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetSingle());
                                    field.SetValue(dataObj, new Vector2(v2Props["x"], v2Props["y"]));
                                    break;
                                case var t when t == typeof(Vector3):
                                    var v3Props = element.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetSingle());
                                    field.SetValue(dataObj, new Vector3(v3Props["x"], v3Props["y"], v3Props["z"]));
                                    break;
                                case var t when t == typeof(Color):
                                    var colorProps = element.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetSingle());
                                    field.SetValue(dataObj, new Color(colorProps["r"], colorProps["g"], colorProps["b"], colorProps["a"]));
                                    break;
                                case var t when t.IsDefined(typeof(SerializableAttribute)):
                                    try { field.SetValue(dataObj, element.Deserialize(t)); }
                                    catch { logger.LogError("Could not deserialized {T}. Content: {S}.", t, element.GetRawText()); }
                                    break;
                                default:
                                    // Unhandled object type
                                    break;
                            }
                            break;
                        case JsonValueKind.Array:
                            switch (field.FieldType)
                            {
                                case var t when t == typeof(List<string>):
                                    field.SetValue(dataObj, element.EnumerateArray().Select(x => x.GetString()).ToList());
                                    break;
                                case var t when t == typeof(string[]):
                                    field.SetValue(dataObj, element.EnumerateArray().Select(x => x.GetString()).ToArray());
                                    break;
                                case var t when t == typeof(int[]):
                                    field.SetValue(dataObj, element.EnumerateArray().Select(x => x.GetInt32()).ToArray());
                                    break;
                                case var t when t == typeof(float[]):
                                    field.SetValue(dataObj, element.EnumerateArray().Select(x => x.GetSingle()).ToArray());
                                    break;
                                default:
                                    if (field.FieldType.IsArray && field.FieldType.GetElementType().IsEnum)
                                    {
                                        var enumType = field.FieldType.GetElementType();
                                        var jsonArray = element.EnumerateArray().ToArray();
                                        var typedArray = Array.CreateInstance(enumType, jsonArray.Length);
                                        for (var i = 0; i < jsonArray.Length; i++)
                                            typedArray.SetValue(Enum.ToObject(enumType, jsonArray[i].GetInt32()), i);
                                        field.SetValue(dataObj, typedArray);
                                    }
                                    else if (field.FieldType.IsArray && field.FieldType.GetElementType().IsDefined(typeof(SerializableAttribute)))
                                    {
                                        var serializableType = field.FieldType.GetElementType();
                                        var serializedArray = element.EnumerateArray().ToArray();
                                        var typedSerializedArray = Array.CreateInstance(serializableType, serializedArray.Length);
                                        for (var i = 0; i < serializedArray.Length; i++)
                                        {
                                            try
                                            {
                                                var elementObject = serializedArray[i].Deserialize(serializableType);
                                                typedSerializedArray.SetValue(elementObject, i);
                                            }
                                            catch
                                            {
                                                logger.LogError("Could not deserialized {T}. Content: {S}.", serializableType, element.GetRawText());
                                            }
                                        }
                                        field.SetValue(dataObj, typedSerializedArray);
                                    }
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    private static void ApplyXMLOverrides(Dictionary<string, string> componentAttributes, object dataObj, FieldInfo[] fields, Microsoft.Extensions.Logging.ILogger logger)
    {
        foreach (var componentValue in componentAttributes.Where(componentValue =>
                     !string.IsNullOrEmpty(componentValue.Value)))
        {
            var field = fields.FirstOrDefault(f => f.Name == componentValue.Key);
            if (field == null)
                continue;

            switch (field.FieldType)
            {
                case var t when t == typeof(string):
                    field.SetValue(dataObj, componentValue.Value);
                    break;
                case var t when t == typeof(int):
                    field.SetValue(dataObj, int.Parse(componentValue.Value));
                    break;
                case var t when t == typeof(bool):
                    field.SetValue(dataObj, componentValue.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase));
                    break;
                case var t when t == typeof(float):
                    field.SetValue(dataObj, float.Parse(componentValue.Value));
                    break;
                case var t when t.IsEnum:
                    field.SetValue(dataObj, Enum.Parse(field.FieldType, componentValue.Value));
                    break;
                case var t when t == typeof(Vector3):
                    var translateComponent = componentValue.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                    var translatedArray = translateComponent.Split(",");
                    field.SetValue(dataObj, new Vector3(float.Parse(translatedArray[0]), float.Parse(translatedArray[1]), float.Parse(translatedArray[2])));
                    break;
                case var t when t == typeof(Vector2):
                    var translateComponent2 = componentValue.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                    var translatedArray2 = translateComponent2.Split(",");
                    field.SetValue(dataObj, new Vector2(float.Parse(translatedArray2[0]), float.Parse(translatedArray2[1])));
                    break;
                case var t when t == typeof(Color):
                    var translateComponent3 = componentValue.Value.Replace("RGBA(", string.Empty).Replace(")", string.Empty);
                    var translatedArray3 = translateComponent3.Split(",");
                    field.SetValue(dataObj, new Color(float.Parse(translatedArray3[0]), float.Parse(translatedArray3[1]), float.Parse(translatedArray3[2]), float.Parse(translatedArray3[3])));
                    break;
                case var t when t == typeof(string[]):
                    var translatedArray4 = componentValue.Value.Split(",");
                    field.SetValue(dataObj, translatedArray4);
                    break;
                default:
                    logger.LogError("It is unknown how to convert a string to a {FieldType} (data: {Data}).",
                        field.FieldType, componentValue.Value);
                    break;
            }
        }
    }
}
