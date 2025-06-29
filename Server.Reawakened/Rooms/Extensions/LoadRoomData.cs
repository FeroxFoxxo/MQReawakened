using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using UnityEngine;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Extensions;

public static class LoadRoomData
{
    public static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public static Dictionary<string, BaseCollider> LoadTerrainColliders(this Room room)
    {
        var outColliderList = new Dictionary<string, BaseCollider>();
        var idCounter = 0;

        foreach (var collider in room.ColliderCatalog.GetTerrainColliders(room.LevelInfo.LevelId))
        {
            idCounter--;

            var id = idCounter.ToString();

            var position = new Vector3(collider.Position.x, collider.Position.y, collider.Position.z);

            outColliderList.Add(id, new TCCollider(id, position, new Rect(0, 0, collider.Width, collider.Height), collider.Plane, room));
        }

        return outColliderList;
    }

    public static Dictionary<string, PlaneModel> LoadPlanes(this LevelInfo levelInfo, Room room, ServerRConfig config)
    {
        var levelInfoPath = Path.Join(config.LevelSaveDirectory, $"{levelInfo.Name}.xml");
        var levelDataPath = Path.Join(config.LevelDataSaveDirectory, $"{levelInfo.Name}.json");

        var xmlDocument = new XmlDocument();
        xmlDocument.Load(levelInfoPath);

        var planeNames = new string[7];

        for (var i = 0; i < planeNames.Length; i++)
            planeNames[i] = (i % 2 != 0 ? "Plane" : "Decor") + i / 2;

        planeNames[5] = "Unity";
        planeNames[6] = "TemplatePlane";

        var planes = planeNames.ToDictionary(name => name, name => new PlaneModel(name));

        foreach (XmlNode data in xmlDocument.FirstChild!.NextSibling!)
            foreach (XmlNode planeNode in data.ChildNodes)
            {
                if (planeNode.Name != "Plane")
                    continue;

                var planeName = planeNode.Attributes!.GetValue("name");
                var plane = planes[planeName];

                foreach (XmlNode gameObject in planeNode.ChildNodes)
                {
                    if (gameObject.Name != "GameObject")
                        continue;

                    foreach (XmlNode gameObjectAttributes in gameObject.ChildNodes)
                    {
                        switch (data.Name)
                        {
                            case "LoadUnit":
                                plane.LoadGameObjectXml(gameObjectAttributes, room);
                                break;
                            case "Collider":
                                plane.LoadColliderXml(gameObjectAttributes);
                                break;
                        }
                    }
                }
            }

        File.WriteAllText(levelDataPath, JsonSerializer.Serialize(planes, _jsonSerializerOptions));

        return planes;
    }

    public static Dictionary<string, List<BaseComponent>> LoadEntities(this Room room, IServiceProvider services)
    {
        var reflectionUtils = services.GetRequiredService<ReflectionUtils>();
        var fileLogger = services.GetRequiredService<FileLogger>();
        var classCopier = services.GetRequiredService<ClassCopier>();
        var assetBundleRConfig = services.GetRequiredService<AssetBundleRConfig>();

        var entities = new Dictionary<string, List<BaseComponent>>();
        room.UnknownEntities = [];

        if (room.Planes == null)
            return entities;

        var entityTransfer = new EntityTransfer(reflectionUtils, classCopier, room, fileLogger, services, assetBundleRConfig);

        foreach (var plane in room.Planes)
        {
            foreach (var entityList in plane.Value.GameObjects)
            {
                var entityId = entityList.Key;
                var components = GetComponents(room, entityList, entityTransfer);

                if (components.Count > 1)
                {
                    if (!room.DuplicateEntities.ContainsKey(entityId))
                        room.DuplicateEntities.Add(entityId, []);

                    foreach (var componentList in components)
                    {
                        room.Logger.LogError("Room already has entity for id {Id}, storing duplicate with components {Components}",
                            entityId, string.Join(", ", componentList.Select(c => c.GetType().Name)));

                        room.DuplicateEntities[entityId].Add(componentList);
                    }
                }
                else if (components.Count == 1)
                {
                    var componentList = components.FirstOrDefault();
                    entities.Add(entityId, componentList);
                }
            }
        }

        return entities;
    }

    private static List<List<BaseComponent>> GetComponents(Room room, KeyValuePair<string, List<GameObjectModel>> objects, EntityTransfer entityTransfer)
    {
        var entityId = objects.Key;
        var models = objects.Value;

        var entities = new List<List<BaseComponent>>();

        foreach (var entity in models)
        {
            var componentList = GetEntity(entity, entityTransfer, out var unknownComponents);

            if (unknownComponents.Count > 0)
            {
                if (!room.UnknownEntities.ContainsKey(entityId))
                    room.UnknownEntities.Add(entityId, []);

                foreach (var component in unknownComponents)
                    if (!room.UnknownEntities[entityId].Contains(component))
                        room.UnknownEntities[entityId].Add(component);
            }

            if (componentList.Count > 0)
                entities.Add(componentList);
        }

        return entities;
    }

    private class EntityTransfer(ReflectionUtils reflectionUtils, ClassCopier classCopier,
        Room room, FileLogger fileLogger, IServiceProvider serviceProvider, AssetBundleRConfig assetBundleRConfig)
    {
        public ReflectionUtils ReflectionUtils => reflectionUtils;
        public ClassCopier ClassCopier => classCopier;
        public Room Room => room;
        public FileLogger FileLogger => fileLogger;
        public IServiceProvider ServiceProvider => serviceProvider;
        public AssetBundleRConfig AssetBundleRConfig => assetBundleRConfig;
    }

    private static List<BaseComponent> GetEntity(GameObjectModel entity, EntityTransfer vars, out List<string> unknownComponents)
    {
        var componentList = new List<BaseComponent>();
        var entityData = new Entity(entity, vars.Room, vars.FileLogger);
        var prefabOverrides = vars.Room.World.GetPrefabOverloads(vars.AssetBundleRConfig, entity.ObjectInfo.PrefabName);

        unknownComponents = [];

        if (prefabOverrides != null)
            foreach (var entry in prefabOverrides)
                if (!entity.ObjectInfo.Components.ContainsKey(entry.Key))
                    entity.ObjectInfo.Components.Add(entry.Key, new ComponentModel
                    {
                        ComponentAttributes = []
                    });

        foreach (var component in entity.ObjectInfo.Components)
        {
            if (!vars.Room.World.ProcessableComponents.TryGetValue(component.Key, out var mqType))
                continue;

            if (!vars.Room.World.EntityComponents.TryGetValue(component.Key, out var internalType))
            {
                unknownComponents.Add(mqType.Name);
                continue;
            }

            var newEntity = vars.ClassCopier.GetClassAndInfo(mqType);

            var dataObj = newEntity.Key;
            var fields = newEntity.Value;

            if (prefabOverrides != null)
                if (prefabOverrides.TryGetValue(mqType.Name, out var value))
                    ApplyPrefabOverrides(value, dataObj, vars.Room.Logger);

            var componentAttributes = component.Value.ComponentAttributes;

            ApplyXMLOverrides(componentAttributes, dataObj, fields, vars.Room.Logger);

            var instancedComponent = vars.ReflectionUtils.CreateBuilder<BaseComponent>(internalType.GetTypeInfo())
                .Invoke(vars.ServiceProvider);

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
                vars.Room.Logger.LogError(
                    "Found invalid {Count} amount of initialization methods for {EntityId} ({EntityType})",
                    methods.Length, entity, internalType.Name);
            else
                methods.First().Invoke(instancedComponent, [dataObj, entityData]);

            componentList.Add(instancedComponent);
        }

        return componentList;
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
                            switch (field.FieldType)
                            {
                                case var t when t == typeof(string):
                                    field.SetValue(dataObj, element.GetString());
                                    continue;
                                default:
                                    logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                                    continue;
                            }
                        case JsonValueKind.Number:
                            switch (field.FieldType)
                            {
                                case var t when t == typeof(int):
                                    field.SetValue(dataObj, element.GetInt32());
                                    continue;
                                case var t when t == typeof(float):
                                    field.SetValue(dataObj, element.GetSingle());
                                    continue;
                                case var t when t == typeof(bool):
                                    field.SetValue(dataObj, element.GetInt32() == 1);
                                    continue;
                                case var t when t.IsEnum:
                                    field.SetValue(dataObj, Enum.ToObject(t, element.GetInt32()));
                                    continue;
                                default:
                                    logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                                    continue;
                            }
                        case JsonValueKind.False:
                            switch (field.FieldType)
                            {
                                case var t when t == typeof(bool):
                                    field.SetValue(dataObj, false);
                                    continue;
                                default:
                                    logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                                    continue;
                            }
                        case JsonValueKind.True:
                            switch (field.FieldType)
                            {
                                case var t when t == typeof(bool):
                                    field.SetValue(dataObj, true);
                                    continue;
                                default:
                                    logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                                    continue;
                            }
                        case JsonValueKind.Null:
                            continue;
                        case JsonValueKind.Object:
                            switch (field.FieldType)
                            {
                                case var t when t == typeof(GameObject) || t == typeof(AnimationClip) || t == typeof(Transform) ||
                                    t == typeof(Mesh) || t == typeof(Texture2D) || t == typeof(Material) || t == typeof(AudioSource) ||
                                    t == typeof(AudioClip) || t == typeof(BoxCollider):
                                    continue;
                                case var t when t == typeof(Vector2):
                                    var v2Props = element.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetSingle());
                                    field.SetValue(dataObj, new Vector2(v2Props["x"], v2Props["y"]));
                                    continue;
                                case var t when t == typeof(Vector3):
                                    var v3Props = element.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetSingle());
                                    field.SetValue(dataObj, new Vector3(v3Props["x"], v3Props["y"], v3Props["z"]));
                                    continue;
                                case var t when t == typeof(Color):
                                    var colorProps = element.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetSingle());
                                    field.SetValue(dataObj, new Color(colorProps["r"], colorProps["g"], colorProps["b"], colorProps["a"]));
                                    continue;
                                case var t when t.IsDefined(typeof(SerializableAttribute)):
                                    try
                                    {
                                        field.SetValue(dataObj, element.Deserialize(t));
                                    }
                                    catch
                                    {
                                        logger.LogError("Could not deserialized {T}. Content: {S}.", t, element.GetRawText());
                                    }
                                    continue;
                                default:
                                    logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                                    continue;
                            }
                        case JsonValueKind.Array:
                            switch (field.FieldType)
                            {
                                case var t when t == typeof(GameObject[]) || t == typeof(AnimationClip[]) || t == typeof(Transform[]) ||
                                    t == typeof(Mesh[]) || t == typeof(Texture2D[]) || t == typeof(Material[]) || t == typeof(AudioSource[]) ||
                                    t == typeof(List<AnimationClip>) || t == typeof(AudioClip[]) || t == typeof(BoxCollider[]):
                                    continue;
                                case var t when t == typeof(List<string>):
                                    field.SetValue(dataObj, element.EnumerateArray().Select(x => x.GetString()).ToList());
                                    continue;
                                case var t when t == typeof(string[]):
                                    field.SetValue(dataObj, element.EnumerateArray().Select(x => x.GetString()).ToArray());
                                    continue;
                                case var t when t == typeof(int[]):
                                    field.SetValue(dataObj, element.EnumerateArray().Select(x => x.GetInt32()).ToArray());
                                    continue;
                                case var t when t == typeof(float[]):
                                    field.SetValue(dataObj, element.EnumerateArray().Select(x => x.GetSingle()).ToArray());
                                    continue;
                                case var t when t.IsArray && t.GetElementType().IsEnum:
                                    var enumType = t.GetElementType();

                                    var jsonArray = element.EnumerateArray().ToArray();
                                    var typedArray = Array.CreateInstance(enumType, jsonArray.Length);

                                    for (var i = 0; i < jsonArray.Length; i++)
                                    {
                                        var enumValue = Enum.ToObject(enumType, jsonArray[i].GetInt32());
                                        typedArray.SetValue(enumValue, i);
                                    }

                                    field.SetValue(dataObj, typedArray);
                                    continue;
                                case var t when t.IsArray && t.GetElementType().IsDefined(typeof(SerializableAttribute)):
                                    var serializableType = t.GetElementType();

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
                                    continue;
                                default:
                                    logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                                    continue;
                            }
                        default:
                            switch (field.FieldType)
                            {
                                default:
                                    logger.LogError("Fields didnt match {T1} {T2}.", element.ValueKind, field.FieldType);
                                    continue;
                            }
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
                    continue;
                case var t when t == typeof(int):
                    field.SetValue(dataObj, int.Parse(componentValue.Value));
                    continue;
                case var t when t == typeof(bool):
                    field.SetValue(dataObj, componentValue.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase));
                    continue;
                case var t when t == typeof(float):
                    field.SetValue(dataObj, float.Parse(componentValue.Value));
                    continue;
                case var t when t.IsEnum:
                    field.SetValue(dataObj, Enum.Parse(field.FieldType, componentValue.Value));
                    continue;
                case var t when t == typeof(Vector3):
                    var translateComponent = componentValue.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                    var translatedArray = translateComponent.Split(",");
                    field.SetValue(dataObj, new Vector3(float.Parse(translatedArray[0]), float.Parse(translatedArray[1]), float.Parse(translatedArray[2])));
                    continue;
                case var t when t == typeof(Vector2):
                    var translateComponent2 = componentValue.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                    var translatedArray2 = translateComponent2.Split(",");
                    field.SetValue(dataObj, new Vector2(float.Parse(translatedArray2[0]), float.Parse(translatedArray2[1])));
                    continue;
                case var t when t == typeof(Color):
                    var translateComponent3 = componentValue.Value.Replace("RGBA(", string.Empty).Replace(")", string.Empty);
                    var translatedArray3 = translateComponent3.Split(",");
                    field.SetValue(dataObj, new Color(float.Parse(translatedArray3[0]), float.Parse(translatedArray3[1]), float.Parse(translatedArray3[2]), float.Parse(translatedArray3[3])));
                    continue;
                case var t when t == typeof(string[]):
                    var translatedArray4 = componentValue.Value.Split(",");
                    field.SetValue(dataObj, translatedArray4);
                    continue;
                default:
                    logger.LogError("It is unknown how to convert a string to a {FieldType} (data: {Data}).",
                        field.FieldType, componentValue.Value);
                    continue;
            }
        }
    }

    public static string GetUnknownComponentTypes(this Room room, string id)
    {
        var entityInfo = new Dictionary<string, IEnumerable<string>>();

        if (room.UnknownEntities.TryGetValue(id, out var value))
            entityInfo.Add("entities", value);

        var components = room.Planes.Values
            .Where(p => p.GameObjects.ContainsKey(id))
            .Select(p => p.GameObjects[id])
            .SelectMany(x => x)
            .SelectMany(g => g.ObjectInfo.Components.Keys)
            .Where(c => !entityInfo.Values.SelectMany(s => s).Contains(c))
            .ToArray();

        if (components.Length != 0)
            entityInfo.Add("components", components);

        entityInfo.Add("game object", [id.ToString()]);

        return $"Unknown {string.Join(", ",
            entityInfo.Select(a => $"{a.Key}: {string.Join(", ", a.Value)}")
        )}";
    }
}
