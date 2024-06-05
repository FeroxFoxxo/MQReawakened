using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Data.Commands;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;
public class InternalInbuiltCommands : InternalXml
{
    public override string BundleName => "InternalInbuiltCommands";

    public override BundlePriority Priority => BundlePriority.Low;

    public List<CommandModel> Commands { get; private set; }

    public override void InitializeVariables() =>
        Commands = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        var commandNodes = xmlDocument.SelectNodes("/Commands/Command");

        Commands.Clear();

        foreach (XmlNode commandNode in commandNodes)
            Commands.Add(CommandModel.FromXmlNode(commandNode));
    }
}
