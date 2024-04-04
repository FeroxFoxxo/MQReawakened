using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalDialogRewrite : InternalXml
{
    public override string BundleName => "InternalDialogRewrite";
    public override BundlePriority Priority => BundlePriority.Medium;

    public Dictionary<string, string> Rewrites;

    public override void InitializeVariables() => Rewrites = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode dialogRewriteXml in xmlDocument.ChildNodes)
        {
            if (dialogRewriteXml.Name != "DialogRewrites") continue;

            foreach (XmlNode dialogRewrite in dialogRewriteXml.ChildNodes)
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

                if (Rewrites.ContainsKey(oldDialogName))
                    continue;

                Rewrites.Add(oldDialogName, newDialogName);
            }
        }
    }
}
