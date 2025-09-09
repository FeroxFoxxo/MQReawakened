using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
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
    public ServerRConfig Config { get; set; }

    private Dictionary<GameVersion, Dictionary<string, string>> _rewrites;
    private GameVersion[] _possibleVersions;

    public override void InitializeVariables()
    {
        _rewrites = [];
        _possibleVersions = [];
    }

    public GameVersion[] GetPossibleVersions() => [.. _rewrites.Keys.Where(v => v <= Config.GameVersion).OrderBy(v => v)];

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

                _rewrites.Add(gameVersion, []);

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

                    _rewrites[gameVersion].Add(oldDialogName, newDialogName);
                }
            }
        }

        _possibleVersions = GetPossibleVersions();
    }

    public string GetRewrite(string dialogName)
    {
        if (_rewrites[Config.GameVersion].TryGetValue(dialogName, out var rewrittenName))
            return rewrittenName;
        else
            foreach (var version in _possibleVersions)
                if (_rewrites[version].TryGetValue(dialogName, out rewrittenName))
                    return rewrittenName;

        return null;
    }
}
