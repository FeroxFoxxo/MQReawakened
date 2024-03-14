using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;
public class WearableSlotFactor : EquipmentSlotFactorXML, IBundledXml<WearableSlotFactor>
{
    public string BundleName => "WearableSlotFactor";

    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<WearableSlotFactor> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<ItemSubCategory, float> SlotFactors;

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<EquipmentSlotFactorXML>("_slotFactorCache", new Dictionary<ItemSubCategory, float>());

        SlotFactors = [];
    }

    public void EditDescription(XmlDocument xml) 
    { 
    }

    public void ReadDescription(string xml) => 
        ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
        GameFlow.EquipmentSlotFactorXML = this;

        SlotFactors = (Dictionary<ItemSubCategory, float>)this.GetField<EquipmentSlotFactorXML>("_slotFactorCache");
    }
}
