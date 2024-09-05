using Server.Base.Accounts.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Data.Commands;
public class ClientCommand(string commandName, string commandDescription, List<ParameterModel> parameters) : CommandModel
{
    public override string CommandName => commandName;
    public override string CommandDescription => commandDescription;
    public override List<ParameterModel> Parameters => parameters;
    public override AccessLevel AccessLevel => AccessLevel.Player;

    public static ClientCommand FromXmlNode(XmlNode node) =>
        new(
            node.Attributes["name"].Value,
            node.Attributes["description"].Value,
            ParameterModel.FromXmlNodes(node.SelectNodes("Parameter"))
        );
}
