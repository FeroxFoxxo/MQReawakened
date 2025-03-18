using System.Xml;

namespace Server.Reawakened.XMLs.Data.ReleaseNotes;
public class ReleaseNote
{
    public string Title { get; set; }
    public string Version { get; set; }
    public string ReleaseDate { get; set; }
    public List<string> Notes { get; set; }

    public ReleaseNote(string title, string version, string releaseDate, List<string> notes)
    {
        Title = title;
        Version = version;
        ReleaseDate = releaseDate;
        Notes = notes;
    }

    public static ReleaseNote FromXmlNode(XmlNode node) =>
        new(
            node.Attributes["title"] == null ? "" : node.Attributes["title"].Value,
            node.Attributes["version"].Value,
            node.Attributes["release_date"].Value,
            FromXmlNodes(node.SelectNodes("Notes"))
        );

    public static List<string> FromXmlNodes(XmlNodeList nodes)
    {
        var releaseNotes = new List<string>();

        foreach (XmlNode node in nodes)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Header":
                        releaseNotes.Add("<h3 class=\"card-title\"><span class=\"game-title\">" + childNode.Attributes["note"].Value + "</h3>");
                        break;
                    case "List":
                        var list = "<li class=\"card-text\">" + childNode.Attributes["note"].Value + "</li>";

                        if (childNode.Attributes["upper_list"] != null
                            && childNode.Attributes["upper_list"].Value == "true")
                            list = "<ul>" + list;
                        else if (childNode.Attributes["bottom_list"] != null
                            && childNode.Attributes["bottom_list"].Value == "true")
                            list += "</ul>";

                        releaseNotes.Add(list);
                        break;
                    case "Line":
                        if (childNode.Attributes["note"] != null)
                        {
                            var line = "<p class=\"card-text\">" + childNode.Attributes["note"].Value + "</p>";

                            releaseNotes.Add(line);
                        }
                        else
                        {
                            var line = "<br />";

                            releaseNotes.Add(line);
                        }
                        break;
                }
            }
        }

        return releaseNotes;
    }
}
