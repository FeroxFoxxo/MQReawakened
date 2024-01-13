using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Server.Reawakened.Players.Models.Minigames;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class ArenaCatalogInt : IBundledXml
{
    public string BundleName => "ArenaCatalogInt";
    public BundlePriority Priority => BundlePriority.Lowest;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, ArenaModel> Arena;

    public ArenaCatalogInt()
    {
    }

    public void InitializeVariables() =>
        Arena = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode arenaCatalog in xmlDocument.ChildNodes)
        {
            if (!(arenaCatalog.Name == "ArenaCatalogs")) continue;

            foreach (XmlNode level in arenaCatalog.ChildNodes)
            {
                if (!(level.Name == "LevelName")) continue;

                var arenaId = -1;
                var timeLimit = -1;
                var isMinigame = false;

                foreach (XmlNode arenaInformation in level.ChildNodes)
                {
                    if (!(arenaInformation.Name == "ArenaInformation")) continue;

                    foreach (XmlAttribute arenaAttribute in arenaInformation.Attributes)
                    switch (arenaAttribute.Name)
                    {
                        case "arenaId":
                            arenaId = int.Parse(arenaAttribute.Value);
                            break;
                        case "timeLimit":
                            timeLimit = int.Parse(arenaAttribute.Value);
                            break;
                        case "isMinigame":
                            isMinigame = bool.Parse(arenaAttribute.Value);
                            break;
                    }

                    var arenaModel = new ArenaModel()
                    {
                        IsMinigame = isMinigame,
                        MinigameTimeLimit = timeLimit
                    };

                    Arena.Add(arenaId, arenaModel);
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }
}
