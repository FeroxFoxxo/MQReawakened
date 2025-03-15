using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Data.News;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalNews : InternalXml
{
    public override string BundleName => "InternalNews";

    public override BundlePriority Priority => BundlePriority.Low;

    public List<NewsData> News { get; private set; }

    public override void InitializeVariables() => News = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        var newsNodes = xmlDocument.SelectNodes("/News/News");

        News.Clear();

        foreach (XmlNode newsData in newsNodes)
            News.Add(NewsData.FromXmlNode(newsData));
    }
}
