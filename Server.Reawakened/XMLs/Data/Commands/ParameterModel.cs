using System.Xml;

namespace Server.Reawakened.XMLs.Data.Commands;
public class ParameterModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<OptionModel> Options { get; set; }

    public static List<ParameterModel> FromXmlNodes(XmlNodeList nodes)
    {
        var parameters = new List<ParameterModel>();
        foreach (XmlNode node in nodes)
        {
            parameters.Add(new ParameterModel
            {
                Name = node.Attributes["name"].Value,
                Description = node.Attributes["description"].Value,
                Options = OptionModel.FromXmlNodes(node.SelectNodes("Option"))
            });
        }

        return parameters;
    }
}
