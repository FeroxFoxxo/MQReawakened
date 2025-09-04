using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Extensions;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalDialogRewrite : InternalXml
{
    public override string BundleName => "InternalDialogRewrite";
    public override BundlePriority Priority => BundlePriority.Medium;

    public ILogger<InternalDialogRewrite> Logger { get; set; }

    public Dictionary<GameVersion, Dictionary<string, string>> Rewrites;

    public override void InitializeVariables() => Rewrites = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode dialogRewriteXml in xmlDocument.ChildNodes)
        {
            if (dialogRewriteXml.Name != "DialogRewrites") continue;

            var gameVersion = GameVersion.Unknown;

            foreach (XmlNode gVXml in dialogRewriteXml.ChildNodes)
            {
                if (!(gVXml.Name == "GameVersion")) continue;

                foreach (XmlAttribute gVAttribute in gVXml.Attributes)
                    switch (gVAttribute.Name)
                    {
                        case "version":
                            gameVersion = gameVersion.GetEnumValue(gVAttribute.Value, Logger);
                            break;
                    }

                Rewrites.Add(gameVersion, []);

                foreach (XmlNode dialogRewrite in gVXml.ChildNodes)
                {
                    if (dialogRewrite.Name != "Dialog") continue;

                    var oldDialogName = string.Empty;
                    var newDialogName = string.Empty;

                    foreach (XmlAttribute dialogRewriteAttribute in dialogRewrite.Attributes)
                        switch (dialogRewriteAttribute.Name)
                        {
                            case "oldDialogName":
                                oldDialogName = dialogRewriteAttribute.Value;
                                continue;
                            case "newDialogName":
                                newDialogName = dialogRewriteAttribute.Value;
                                continue;
                        }

                    Rewrites[gameVersion].Add(oldDialogName, newDialogName);
                }
            }
        }
    }
}
