using System.Xml;

namespace Server.Reawakened.XMLs.Data.Commands;
public class ParameterModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Optional { get; set; }
    public List<OptionModel> Options { get; set; }

    public static List<ParameterModel> FromXmlNodes(XmlNodeList nodes)
    {
        var parameters = new List<ParameterModel>();
        foreach (XmlNode node in nodes)
        {
            var optional = node.Attributes["optional"];
            parameters.Add(new ParameterModel
            {
                Name = node.Attributes["name"].Value,
                Description = node.Attributes["description"].Value,
                Optional = optional != null && optional.Value == "true",
                Options = OptionModel.FromXmlNodes(node.SelectNodes("Option"))
            });
        }

        return parameters;
    }
}
