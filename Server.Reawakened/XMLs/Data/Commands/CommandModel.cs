using System.Xml;

namespace Server.Reawakened.XMLs.Data.Commands;
public class CommandModel
{
    public string CommandName { get; set; }
    public string CommandDescription { get; set; }
    public List<ParameterModel> Parameters { get; set; }

    public static CommandModel FromXmlNode(XmlNode node)
    {
        var command = new CommandModel
        {
            CommandName = node.Attributes["name"].Value,
            CommandDescription = node.Attributes["description"].Value,
            Parameters = ParameterModel.FromXmlNodes(node.SelectNodes("Parameter"))
        };

        return command;
    }
}
