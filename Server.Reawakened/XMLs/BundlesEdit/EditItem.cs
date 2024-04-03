using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesEdit;
public class EditItem : InternalXml
{
    public override string BundleName => "EditItem";
    public override BundlePriority Priority => BundlePriority.Highest;

    public ILogger<EditItem> Logger { get; set; }
    public ServerRConfig Config { get; set; }

    private Dictionary<GameVersion, Dictionary<string, Dictionary<string, string>>> _editedItemAttributes;
    private GameVersion[] _possibleVersions;

    public override void InitializeVariables()
    {
        _editedItemAttributes = [];
        _possibleVersions = [];
    }
    public GameVersion[] GetPossibleVersions() => [.. _editedItemAttributes.Keys.Where(v => v <= Config.GameVersion).OrderBy(v => v)];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode items in xmlDocument.ChildNodes)
        {
            if (!(items.Name == "EditedItems")) continue;

            foreach (XmlNode gVXml in items.ChildNodes)
            {
                if (!(gVXml.Name == "GameVersion")) continue;

                var gameVersion = GameVersion.Unknown;

                foreach (XmlAttribute gVAttribute in gVXml.Attributes)
                    switch (gVAttribute.Name)
                    {
                        case "version":
                            gameVersion = gameVersion.GetEnumValue(gVAttribute.Value, Logger);
                            break;
                    }

                _editedItemAttributes.Add(gameVersion, []);

                foreach (XmlNode item in gVXml.ChildNodes)
                {
                    if (!(item.Name == "Item")) continue;

                    var name = string.Empty;
                    var itemId = string.Empty;

                    foreach (XmlAttribute itemAttributes in item.Attributes)
                        switch (itemAttributes.Name)
                        {
                            case "name":
                                name = itemAttributes.Value;
                                break;
                        }

                    _editedItemAttributes[gameVersion].Add(name, []);

                    foreach (XmlNode itemAttribute in item.ChildNodes)
                    {
                        if (!(itemAttribute.Name == "EditAttribute")) continue;

                        var key = string.Empty;
                        var value = string.Empty;

                        foreach (XmlAttribute itemAttributes in itemAttribute.Attributes)
                            switch (itemAttributes.Name)
                            {
                                case "key":
                                    key = itemAttributes.Value;
                                    break;
                                case "value":
                                    value = itemAttributes.Value;
                                    break;
                            }

                        _editedItemAttributes[gameVersion][name].Add(key, value);
                    }
                }
            }
        }

        _possibleVersions = GetPossibleVersions();
    }

    public void EditItemAttributes(string prefabName, XmlNode xmlElement)
    {
        foreach (var version in _possibleVersions)
            if (_editedItemAttributes[version].TryGetValue(prefabName, out var editedAttributes))
                foreach (XmlAttribute itemAttributes in xmlElement.Attributes)
                    if (editedAttributes.TryGetValue(itemAttributes.Name, out var value))
                        itemAttributes.Value = value;
    }

    public Dictionary<string, string> GetItemAttributes(string prefabName)
    {
        Dictionary<string, string> attributes = [];

        foreach (var version in _possibleVersions)
            if (_editedItemAttributes[version].TryGetValue(prefabName, out var editedAttributes))
                foreach (var attribute in editedAttributes)
                {
                    if (attributes.ContainsKey(attribute.Key))
                        attributes[attribute.Key] = attribute.Value;
                    else
                        attributes.Add(attribute.Key, attribute.Value);
                }

        return attributes;
    }
}
