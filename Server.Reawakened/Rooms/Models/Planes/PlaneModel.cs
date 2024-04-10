using Server.Reawakened.Rooms.Extensions;
using System.Xml;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Planes;

public class PlaneModel(string planeName)
{
    public Dictionary<string, List<GameObjectModel>> GameObjects { get; set; } = [];
    public string PlaneName => planeName;

    public void LoadColliderXml(XmlNode colliderNode)
    {
        var id = colliderNode.Attributes!.GetValue("id");

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

        var rect = colliderList.GetSurroundingRect();

        foreach (var obj in value)
            obj.ObjectInfo.Rectangle = rect;
    }

    public void LoadGameObjectXml(XmlNode gameObjectNode, Room room)
    {
        var attributes = gameObjectNode.Attributes!;

        if (attributes == null)
            return;

        var prefabName = attributes.GetValue("name");
        var id = PlaneName.Equals("TemplatePlane") ? prefabName : attributes.GetValue("id");

        var objectInfo = new ObjectInfoModel
        {
            PrefabName = prefabName,
            ObjectId = id,
            Position = new Vector3Model(
                attributes.GetSingleValue("x"),
                attributes.GetSingleValue("y"),
                attributes.GetSingleValue("z"),
                id,
                room
            ),
            Rotation = new Vector3Model
            (
                attributes.GetSingleValue("rx"),
                attributes.GetSingleValue("ry"),
                attributes.GetSingleValue("rz"),
                string.Empty,
                null
            ),
            Scale = new Vector3Model
            (
                attributes.GetSingleValue("sx"),
                attributes.GetSingleValue("sy"),
                attributes.GetSingleValue("sz"),
                string.Empty,
                null
            ),
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

        if (!GameObjects.ContainsKey(id))
            GameObjects.Add(id, []);

        GameObjects[id].Add(obj);
    }
}
