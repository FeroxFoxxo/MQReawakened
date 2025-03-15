using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Data.ReleaseNotes;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalReleaseNotes : InternalXml
{
    public override string BundleName => "InternalReleaseNotes";

    public override BundlePriority Priority => BundlePriority.Low;

    public List<ReleaseNote> ReleaseNotes { get; private set; }

    public override void InitializeVariables() => ReleaseNotes = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        var releaseNoteNodes = xmlDocument.SelectNodes("/Releases/Release");

        ReleaseNotes.Clear();

        foreach (XmlNode releaseNote in releaseNoteNodes)
            ReleaseNotes.Add(ReleaseNote.FromXmlNode(releaseNote));
    }
}
