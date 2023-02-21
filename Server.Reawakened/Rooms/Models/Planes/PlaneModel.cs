using Server.Reawakened.Rooms.Extensions;
using System.Xml;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Planes;

public class PlaneModel
{
    public Dictionary<int, GameObjectModel> GameObjects { get; set; }

    public PlaneModel() => GameObjects = new Dictionary<int, GameObjectModel>();

    public void LoadColliderXml(XmlNode colliderNode)
    {
        var id = int.Parse(colliderNode.Attributes!.GetNamedItem("id")!.Value!);

        var colliderList = (from XmlNode collider in colliderNode.ChildNodes
            where collider.Name == "vertex"
            select collider.Attributes!
            into vertex
            select new Vector2
            {
                x = Convert.ToSingle(vertex.GetNamedItem("x")!.Value),
                y = Convert.ToSingle(vertex.GetNamedItem("y")!.Value)
            }).ToArray();

        if (!GameObjects.ContainsKey(id))
            return;

        GameObjects[id].Rect = colliderList.GetSurroundingRect();
    }

    public void LoadGameObjectXml(XmlNode gameObjectNode)
    {
        var attributes = gameObjectNode.Attributes!;

        var objectInfo = new ObjectInfoModel
        {
            PrefabName = attributes.GetNamedItem("name")!.Value,
            ObjectId = Convert.ToInt32(attributes.GetNamedItem("id")!.Value),
            Position = new Vector3Model
            {
                X = Convert.ToSingle(attributes.GetNamedItem("x")!.Value),
                Y = Convert.ToSingle(attributes.GetNamedItem("y")!.Value),
                Z = Convert.ToSingle(attributes.GetNamedItem("z")!.Value)
            },
            Rotation = new Vector3Model
            {
                X = Convert.ToSingle(attributes.GetNamedItem("rx")!.Value),
                Y = Convert.ToSingle(attributes.GetNamedItem("ry")!.Value),
                Z = Convert.ToSingle(attributes.GetNamedItem("rz")!.Value)
            },
            Scale = new Vector3Model
            {
                X = Convert.ToSingle(attributes.GetNamedItem("sx")!.Value),
                Y = Convert.ToSingle(attributes.GetNamedItem("sy")!.Value),
                Z = Convert.ToSingle(attributes.GetNamedItem("sz")!.Value)
            }
        };

        foreach (XmlNode componentNode in gameObjectNode.ChildNodes)
        {
            if (componentNode.Name != "component")
                continue;

            var componentName = componentNode.Attributes!.GetNamedItem("name")!.Value!;

            var component = new ComponentModel();

            foreach (XmlNode componentAttribute in componentNode.ChildNodes)
            {
                var componentAttributeName = componentAttribute.Attributes!.GetNamedItem("name")!.Value!;
                component.ComponentAttributes.Add(componentAttributeName, componentAttribute.InnerText);
            }

            objectInfo.Components[componentName] = component;
        }

        var obj = new GameObjectModel
        {
            ObjectInfo = objectInfo
        };

        if (GameObjects.ContainsKey(obj.ObjectInfo.ObjectId))
            return;

        GameObjects.Add(obj.ObjectInfo.ObjectId, obj);
    }
}
