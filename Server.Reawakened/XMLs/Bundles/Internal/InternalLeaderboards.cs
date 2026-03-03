using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Extensions;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;
public class InternalLeaderboards : InternalXml
{
    public override string BundleName => "InternalLeaderboards";

    public override BundlePriority Priority => BundlePriority.High;

    public ILogger<InternalLeaderboards> Logger { get; set; }

    public List<LeaderBoardGameJson.Game> Games;

    public override void InitializeVariables() => Games = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode xmlNode in xmlDocument.ChildNodes)
        {
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                short id = 1;
                var name = string.Empty;
                var sortDirection = string.Empty;
                var scoreType = string.Empty;
                short maxScores = 10;
                var ranked = false;

                foreach (XmlAttribute attribute in childNode.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "id":
                            id = short.Parse(attribute.Value);
                            break;
                        case "name":
                            name = attribute.Value;
                            break;
                        case "sortDirection":
                            sortDirection = attribute.Value;
                            break;
                        case "scoreType":
                            scoreType = attribute.Value;
                            break;
                        case "maxScores":
                            maxScores = short.Parse(attribute.Value);
                            break;
                        case "ranked":
                            ranked = ranked.GetBoolValue(attribute.Value, Logger);
                            break;
                    }
                }

                Games.Add(new LeaderBoardGameJson.Game
                {
                    id = id,
                    name = name,
                    sortDirection = sortDirection,
                    scoreType = scoreType,
                    maxScores = maxScores,
                    ranked = ranked
                });
            }
        }
    }
}
