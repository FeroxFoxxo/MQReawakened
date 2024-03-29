﻿using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;
public class RarityFactor : RarityFactorXML, IBundledXml<RarityFactor>
{
    public string BundleName => "RarityFactor";

    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<RarityFactor> Logger { get; set; }
    public IServiceProvider Services { get; set; }

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
