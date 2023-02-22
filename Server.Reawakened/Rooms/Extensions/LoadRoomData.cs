using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Xml;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Extensions;

public static class LoadRoomData
{
    public static Dictionary<string, PlaneModel> LoadPlanes(this LevelInfo levelInfo, ServerStaticConfig config)
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

        var planes = planeNames.ToDictionary(name => name, name => new PlaneModel());

        foreach (XmlNode data in xmlDocument.FirstChild!.NextSibling!)
        foreach (XmlNode planeNode in data.ChildNodes)
        {
            if (planeNode.Name != "Plane")
                continue;

            var planeName = planeNode.Attributes!.GetNamedItem("name")!.Value!;
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
                            plane.LoadGameObjectXml(gameObjectAttributes);
                            break;
                        case "Collider":
                            plane.LoadColliderXml(gameObjectAttributes);
                            break;
                    }
                }
            }
        }

        File.WriteAllText(levelDataPath,
            JsonSerializer.Serialize(planes, new JsonSerializerOptions { WriteIndented = true }));

        return planes;
    }

    public static Dictionary<int, List<BaseSyncedEntity>> LoadEntities(this Room room, ReflectionUtils reflectionUtils,
        FileLogger fileLogger, IServiceProvider services, out Dictionary<int, List<string>> unknownEntities)
    {
        var entities = new Dictionary<int, List<BaseSyncedEntity>>();
        unknownEntities = new Dictionary<int, List<string>>();

        if (room.Planes == null)
            return entities;

        var invalidProcessable = new List<string>();

        var syncedEntities = typeof(BaseSyncedEntity).Assembly.GetServices<BaseSyncedEntity>()
            .Where(t => t.BaseType != null)
            .Where(t => t.BaseType.GenericTypeArguments.Length > 0)
            .ToDictionary(t => t.BaseType.GenericTypeArguments.First().FullName, t => t);

        var processable = typeof(DataComponentAccessor).Assembly.GetServices<DataComponentAccessor>()
            .ToDictionary(x => x.Name, x => x);

        foreach (var plane in room.Planes)
        foreach (var entity in plane.Value.GameObjects)
        foreach (var component in entity.Value.ObjectInfo.Components)
        {
            if (!processable.TryGetValue(component.Key, out var mqType))
                continue;

            if (syncedEntities.TryGetValue(mqType.FullName!, out var internalType))
            {
                var dataObj = FormatterServices.GetUninitializedObject(mqType);

                var fields = mqType.GetFields()
                    .Where(prop => prop.IsDefined(typeof(MQAttribute), false))
                    .ToArray();

                foreach (var componentValue in component.Value.ComponentAttributes.Where(componentValue =>
                             !string.IsNullOrEmpty(componentValue.Value)))
                {
                    var field = fields.FirstOrDefault(f => f.Name == componentValue.Key);

                    if (field == null)
                        continue;

                    if (field.FieldType == typeof(string))
                        field.SetValue(dataObj, componentValue.Value);
                    else if (field.FieldType == typeof(int))
                        field.SetValue(dataObj, int.Parse(componentValue.Value));
                    else if (field.FieldType == typeof(bool))
                        field.SetValue(dataObj, componentValue.Value.ToLower() == "true");
                    else if (field.FieldType == typeof(float))
                        field.SetValue(dataObj, float.Parse(componentValue.Value));
                    else if (field.FieldType.IsEnum)
                        field.SetValue(dataObj, Enum.Parse(field.FieldType, componentValue.Value));
                    else
                        room.Logger.LogError("It is unknown how to convert a string to a {FieldType}.",
                            field.FieldType);
                }

                var storedData = new StoredEntityModel(entity.Value, room, fileLogger);

                var instancedEntity = reflectionUtils.CreateBuilder<BaseSyncedEntity>(internalType.GetTypeInfo())
                    .Invoke(services);

                var methods = internalType.GetMethods().Where(m =>
                {
                    var parameters = m.GetParameters();

                    return
                        m.Name == "SetEntityData" &&
                        parameters.Length == 2 &&
                        parameters[0].ParameterType == dataObj.GetType() &&
                        parameters[1].ParameterType == storedData.GetType();
                }).ToArray();

                if (methods.Length != 1)
                    room.Logger.LogError(
                        "Found invalid {Count} amount of initialization methods for {EntityId} ({EntityType})",
                        methods.Length, entity.Key, internalType.Name);
                else
                    methods.First().Invoke(instancedEntity, new[] { dataObj, storedData });

                if (!entities.ContainsKey(entity.Key))
                    entities.Add(entity.Key, new List<BaseSyncedEntity>());

                entities[entity.Key].Add(instancedEntity);
            }
            else if (!invalidProcessable.Contains(mqType.Name))
            {
                if (!unknownEntities.ContainsKey(entity.Key))
                    unknownEntities.Add(entity.Key, new List<string>());

                unknownEntities[entity.Key].Add(mqType.Name);

                invalidProcessable.Add(mqType.Name);
            }
        }

        foreach (var type in invalidProcessable.Order())
            room.Logger.LogWarning("Could not find synced entity for {EntityType}", type);

        return entities;
    }

    public static Dictionary<int, T> GetEntities<T>(this Room room) where T : class
    {
        var type = typeof(T);

        var entities = room.Entities.Values.SelectMany(t => t).Where(t => t is T).ToArray();

        if (entities.Length > 0)
            return entities.ToDictionary(x => x.Id, x => x as T);

        room.Logger.LogError("Could not find entity with type {TypeName}. Returning empty. " +
                             "Possible types: {Types}", type.Name, string.Join(", ", room.Entities.Keys));

        return new Dictionary<int, T>();
    }

    public static string GetUnknownEntityTypes(this Room room, int id)
    {
        var entityInfo = new Dictionary<string, IEnumerable<string>>();

        if (room.UnknownEntities.TryGetValue(id, out var value))
            entityInfo.Add("entities", value);

        var components = room.Planes.Values
            .Where(p => p.GameObjects.ContainsKey(id))
            .Select(p => p.GameObjects[id])
            .SelectMany(g => g.ObjectInfo.Components.Keys)
            .Where(c => !entityInfo.Values.SelectMany(s => s).Contains(c))
            .ToArray();

        if (components.Any())
            entityInfo.Add("components", components);

        entityInfo.Add("game object", new[] { id.ToString() });

        return $"Unknown {string.Join(", ",
            entityInfo.Select(a => $"{a.Key}: {string.Join(", ", a.Value)}")
        )}";
    }
}
