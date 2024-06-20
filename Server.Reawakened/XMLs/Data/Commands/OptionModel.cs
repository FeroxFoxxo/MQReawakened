using System.Xml;

namespace Server.Reawakened.XMLs.Data.Commands;
public class OptionModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ParameterModel> Parameters { get; set; }

    public static List<OptionModel> FromXmlNodes(XmlNodeList nodes)
    {
        var options = new List<OptionModel>();
        foreach (XmlNode node in nodes)
            options.Add(new OptionModel
            {
                Name = node.Attributes["name"].Value,
                Description = node.Attributes["description"].Value,
                Parameters = ParameterModel.FromXmlNodes(node.SelectNodes("Parameter"))
            });

        return options;
    }
}
