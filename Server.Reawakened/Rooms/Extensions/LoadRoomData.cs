using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using System.Text.Json;
using System.Xml;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Extensions;

public static class LoadRoomData
{
    public static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public static void LoadTerrainColliders(this Room room)
    {
        var colliderIndex = 0;
        foreach (var collider in room.ColliderCatalog.GetTerrainColliders(room.LevelInfo.LevelId))
        {
            var position = new Vector3Model(collider.Position.x, collider.Position.y, collider.Position.z);
            var box = new RectModel(0, 0, collider.Width, collider.Height);

            _ = new TCCollider(colliderIndex.ToString(), position, box, collider.Plane, room);
        }
    }

    public static Dictionary<string, PlaneModel> LoadPlanes(this LevelInfo levelInfo, ServerRConfig config)
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
                                plane.LoadGameObjectXml(gameObjectAttributes);
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
        var builder = services.GetRequiredService<EntityComponentBuilder>();

        var entities = new Dictionary<string, List<BaseComponent>>();
        room.UnknownEntities = [];

        if (room.Planes == null)
            return entities;

        foreach (var plane in room.Planes)
        {
            foreach (var entityList in plane.Value.GameObjects)
            {
                var entityId = entityList.Key;
                var components = GetComponents(room, entityList, builder);

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

    private static List<List<BaseComponent>> GetComponents(Room room, KeyValuePair<string, List<GameObjectModel>> objects, EntityComponentBuilder builder)
    {
        var entityId = objects.Key;
        var models = objects.Value;

        var entities = new List<List<BaseComponent>>();

        foreach (var entity in models)
        {
            var componentList = builder.Build(entity, room, out var unknownComponents);

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


    public static string GetUnknownComponentTypes(this Room room, string id)
    {
        var entityInfo = new Dictionary<string, IEnumerable<string>>();

        if (room.UnknownEntities != null && room.UnknownEntities.TryGetValue(id, out var value))
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
