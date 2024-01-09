using Server.Reawakened.Rooms.Extensions;
using System.Xml;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Planes;

public class PlaneModel(string planeName)
{
    public Dictionary<int, GameObjectModel> GameObjects { get; set; } = [];
    public string PlaneName { get; } = planeName;

    public void LoadColliderXml(XmlNode colliderNode)
    {
        var id = colliderNode.Attributes!.GetIntValue("id");

        var colliderList = (from XmlNode collider in colliderNode.ChildNodes
                            where collider.Name == "vertex"
                            select collider.Attributes!
            into vertex
                            select new Vector2
                            {
                                x = vertex.GetSingleValue("x"),
                                y = vertex.GetSingleValue("y")
                            }).ToArray();

        if (!GameObjects.TryGetValue(id, out var value))
            return;
        value.ObjectInfo.Rectangle = colliderList.GetSurroundingRect();
    }

    public void LoadGameObjectXml(XmlNode gameObjectNode)
    {
        var attributes = gameObjectNode.Attributes!;

        if (attributes == null)
            return;

        var objectInfo = new ObjectInfoModel
        {
            PrefabName = attributes.GetValue("name"),
            ObjectId = attributes.GetIntValue("id"),
            Position = new Vector3Model
            {
                X = attributes.GetSingleValue("x"),
                Y = attributes.GetSingleValue("y"),
                Z = attributes.GetSingleValue("z")
            },
            Rotation = new Vector3Model
            {
                X = attributes.GetSingleValue("rx"),
                Y = attributes.GetSingleValue("ry"),
                Z = attributes.GetSingleValue("rz")
            },
            Scale = new Vector3Model
            {
                X = attributes.GetSingleValue("sx"),
                Y = attributes.GetSingleValue("sy"),
                Z = attributes.GetSingleValue("sz")
            },
            ParentPlane = PlaneName,
            Rectangle = new RectModel(-1000.0f, -1000.0f, 0f, 0f)

        };

        foreach (XmlNode componentNode in gameObjectNode.ChildNodes)
        {
            if (componentNode.Name != "component")
                continue;

            var componentName = componentNode.Attributes!.GetValue("name");

            var component = new ComponentModel();

            foreach (XmlNode componentAttribute in componentNode.ChildNodes)
            {
                var componentAttributeName = componentAttribute.Attributes!.GetValue("name");
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
