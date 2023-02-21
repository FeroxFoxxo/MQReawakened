using Server.Reawakened.Rooms.Models.Planes;
using System.Xml;

namespace Server.Reawakened.Rooms.Extensions;

public static class GetPlanes
{
    public static Dictionary<string, PlaneModel> LoadPlanes(this XmlDocument doc)
    {
        var planeNames = new string[7];

        for (var i = 0; i < planeNames.Length; i++)
            planeNames[i] = (i % 2 != 0 ? "Plane" : "Decor") + i / 2;

        planeNames[5] = "Unity";
        planeNames[6] = "TemplatePlane";

        var planes = planeNames.ToDictionary(name => name, name => new PlaneModel());

        foreach (XmlNode data in doc.FirstChild!.NextSibling!)
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

        return planes;
    }
}
