using System.Xml;

namespace Server.Reawakened.XMLs.Data.News;
public class NewsData
{
    public bool DefaultNews { get; set; }
    public string NewsDate { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public List<string> Notes { get; set; }

    public NewsData(bool defaultNews, string newsDate, string startDate, string endDate, List<string> notes)
    {
        DefaultNews = defaultNews;
        NewsDate = newsDate;
        StartDate = startDate;
        EndDate = endDate;
        Notes = notes;
    }

    public static NewsData FromXmlNode(XmlNode node) =>
        new(
            node.Attributes["default"] != null && bool.Parse(node.Attributes["default"].Value),
            "\u2022 " + node.Attributes["news_date"].Value.Replace("{CurrentDate}", DateTime.Now.ToShortDateString()),
            node.Attributes["start_date"] == null ? "" : node.Attributes["start_date"].Value,
            node.Attributes["end_date"] == null ? "" : node.Attributes["end_date"].Value,
            FromXmlNodes(node.SelectNodes("Lines"))
        );

    public static List<string> FromXmlNodes(XmlNodeList nodes)
    {
        var news = new List<string>();

        foreach (XmlNode node in nodes)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "List":
                        var list = "\u2022 " + childNode.Attributes["note"].Value;

                        news.Add(list.Replace("{CurrentYear}", DateTime.Now.Year.ToString()));
                        break;
                    case "Line":
                        var note = childNode.Attributes["note"].Value;

                        news.Add(note.Replace("{CurrentYear}", DateTime.Now.Year.ToString()));
                        break;
                }
            }
        }

        return news;
    }
}
