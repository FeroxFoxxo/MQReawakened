using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using UnityEngine;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Extensions;

public static class LoadRoomData
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

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

        var entities = new Dictionary<string, List<BaseComponent>>();
        room.UnknownEntities = [];

        if (room.Planes == null)
            return entities;

        var entityTransfer = new EntityTransfer(reflectionUtils, classCopier, room, fileLogger, services);

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
        Room room, FileLogger fileLogger, IServiceProvider serviceProvider)
    {
        public ReflectionUtils ReflectionUtils => reflectionUtils;
        public ClassCopier ClassCopier => classCopier;
        public Room Room => room;
        public FileLogger FileLogger => fileLogger;
        public IServiceProvider ServiceProvider => serviceProvider;
    }

    private static List<BaseComponent> GetEntity(GameObjectModel entity, EntityTransfer vars, out List<string> unknownComponents)
    {
        var componentList = new List<BaseComponent>();
        var entityData = new Entity(entity, vars.Room, vars.FileLogger);

        unknownComponents = [];

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
                    field.SetValue(dataObj, componentValue.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase));
                else if (field.FieldType == typeof(float))
                    field.SetValue(dataObj, float.Parse(componentValue.Value));
                else if (field.FieldType.IsEnum)
                    field.SetValue(dataObj, Enum.Parse(field.FieldType, componentValue.Value));
                else if (field.FieldType == typeof(Vector3))
                {
                    var translateComponent = componentValue.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                    var translatedArray = translateComponent.Split(",");
                    field.SetValue(dataObj, new Vector3(float.Parse(translatedArray[0]), float.Parse(translatedArray[1]), float.Parse(translatedArray[2])));
                }
                else if (field.FieldType == typeof(Vector2))
                {
                    var translateComponent = componentValue.Value.Replace("(", string.Empty).Replace(")", string.Empty);
                    var translatedArray = translateComponent.Split(",");
                    field.SetValue(dataObj, new Vector2(float.Parse(translatedArray[0]), float.Parse(translatedArray[1])));
                }
                else if (field.FieldType == typeof(Color))
                {
                    var translateComponent = componentValue.Value.Replace("RGBA(", string.Empty).Replace(")", string.Empty);
                    var translatedArray = translateComponent.Split(",");
                    field.SetValue(dataObj, new Color(float.Parse(translatedArray[0]), float.Parse(translatedArray[1]), float.Parse(translatedArray[2]), float.Parse(translatedArray[3])));
                }
                else if (field.FieldType == typeof(string[]))
                {
                    var translatedArray = componentValue.Value.Split(",");
                    field.SetValue(dataObj, translatedArray);
                }
                else
                {
                    vars.Room.Logger.LogError("It is unknown how to convert a string to a {FieldType} (data: {Data}).",
                        field.FieldType, componentValue.Value);
                }
            }

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
