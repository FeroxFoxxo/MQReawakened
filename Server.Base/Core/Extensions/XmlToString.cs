using System.Xml;

namespace Server.Base.Core.Extensions;

public static class XmlToString
{
    public static string WriteToString(this XmlDocument document)
    {
        using var stringWriter = new StringWriter();

        using var xmlTextWriter = XmlWriter.Create(stringWriter,
            new XmlWriterSettings { Indent = true });

        document.WriteTo(xmlTextWriter);
        xmlTextWriter.Flush();

        return stringWriter.GetStringBuilder().ToString();
    }
}
