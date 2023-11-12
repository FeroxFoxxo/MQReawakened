using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class DialogRewriteInt : IBundledXml
{
    public string BundleName => "DialogRewriteInt";
    public BundlePriority Priority => BundlePriority.Medium;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<string, string> Rewrites;

    public void InitializeVariables() => Rewrites = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode dialogRewrites in xmlDocument.ChildNodes)
        {
            if (dialogRewrites.Name != "DialogRewrites") continue;

            foreach (XmlNode dialogXml in dialogRewrites.ChildNodes)
            {
                if (dialogXml.Name != "Dialog") continue;

                var oldDialogName = string.Empty;
                var newDialogName = string.Empty;

                foreach (XmlAttribute attributes in dialogXml.Attributes!)
                    switch (attributes.Name)
                    {
                        case "oldDialogName":
                            oldDialogName = attributes.Value;
                            continue;
                        case "newDialogName":
                            newDialogName = attributes.Value;
                            continue;
                    }

                if (Rewrites.ContainsKey(oldDialogName))
                    continue;

                Rewrites.Add(oldDialogName, newDialogName);
            }
        }
    }

    public void FinalizeBundle()
    {
    }
}
