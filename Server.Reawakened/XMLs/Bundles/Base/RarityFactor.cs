using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Base;
public class RarityFactor : RarityFactorXML, IBundledXml
{
    public string BundleName => "RarityFactor";

    public BundlePriority Priority => BundlePriority.Low;

    public Dictionary<ItemRarity, float> OffensiveFactor;
    public Dictionary<ItemRarity, float> DefensiveFactor;

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<RarityFactorXML>("_rarityOffensiveFactorCache", new Dictionary<ItemRarity, float>());
        this.SetField<RarityFactorXML>("_rarityDefensiveFactorCache", new Dictionary<ItemRarity, float>());

        OffensiveFactor = [];
        DefensiveFactor = [];
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
        GameFlow.RarityFactorXML = this;

        OffensiveFactor = (Dictionary<ItemRarity, float>)this.GetField<RarityFactorXML>("_rarityOffensiveFactorCache");
        DefensiveFactor = (Dictionary<ItemRarity, float>)this.GetField<RarityFactorXML>("_rarityDefensiveFactorCache");
    }
}
