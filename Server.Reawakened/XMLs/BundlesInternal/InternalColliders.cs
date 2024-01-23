using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using Server.Reawakened.XMLs.Models.Npcs;
using System.Xml;
using UnityEngine;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalColliders : IBundledXml<InternalColliders>
{
    public string BundleName => "InternalColliders";
    public BundlePriority Priority => BundlePriority.High;

    public ILogger<InternalColliders> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, List<ColliderModel>> TerrainColliderCatalog;

    public void InitializeVariables() => TerrainColliderCatalog = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode colliderXml in xmlDocument.ChildNodes)
        {
            if (colliderXml.Name != "InternalColliders") continue;

            foreach (XmlNode level in colliderXml.ChildNodes)
            {
                if (level.Name != "Level") continue;

                var levelId = -1;
                var colliderList = new List<ColliderModel>();

                foreach (XmlAttribute levelAttribute in level.Attributes)
                    if (levelAttribute.Name == "id")
                    {
                        levelId = int.Parse(levelAttribute.Value);
                        continue;
                    }
                foreach (XmlNode levelPlane in level.ChildNodes)
                {
                    var plane = "Plane0";

                    foreach (XmlAttribute planeAttribute in levelPlane.Attributes)
                        if (planeAttribute.Name == "plane")
                        {
                            plane = (planeAttribute.Value == "Plane1") ? "Plane1" : "Plane0";
                            continue;
                        }

                    foreach (XmlNode colliderBox in levelPlane.ChildNodes)
                    {
                        var posX = -100f;
                        var posY = -100f;
                        var width = 1f;
                        var height = 1f;

                        foreach (XmlAttribute colliderAttributes in colliderBox.Attributes)
                        {
                            switch (colliderAttributes.Name)
                            {
                                case "x":
                                    posX = float.Parse(colliderAttributes.Value);
                                    continue;
                                case "y":
                                    posY = float.Parse(colliderAttributes.Value);
                                    continue;
                                case "width":
                                    width = float.Parse(colliderAttributes.Value);
                                    continue;
                                case "height":
                                    height = float.Parse(colliderAttributes.Value);
                                    continue;
                            }

                            colliderList.Add(new ColliderModel(plane, posX, posY, width, height));

                        }
                    }
                }
                TerrainColliderCatalog.Add(levelId, colliderList);
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public List<ColliderModel> GetTerrainColliders(int id) =>
        TerrainColliderCatalog.TryGetValue(id, out var colliders) ? colliders : [];
}
