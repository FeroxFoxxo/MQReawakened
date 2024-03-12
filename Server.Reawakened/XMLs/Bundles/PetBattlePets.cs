using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;
public class PetBattlePets : PetBattlePetsXML, IBundledXml<PetBattlePets>
{
    public string BundleName => "PetBattlePets";
    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<PetBattlePets> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        PetBattlePetsDictionary = [];
        PetBattlePetList = [];
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlElement item in xmlDocument.GetElementsByTagName("petBattlePet"))
        {
            var petBattlePet = new PetBattlePet();

            foreach (XmlAttribute attribute in item.Attributes) 
            {
                switch (attribute.Name)
                {
                    case "id":
                        petBattlePet.id = int.Parse(attribute.Value);
                        break;
                    case "itemId":
                        petBattlePet.itemId = int.Parse(attribute.Value);
                        break;
                    case "health":
                        petBattlePet.health = int.Parse(attribute.Value);
                        break;
                    case "accuracy":
                        petBattlePet.accuracy = int.Parse(attribute.Value);
                        break;
                    case "tier":
                        petBattlePet.tier = int.Parse(attribute.Value);
                        break;
                    case "species":
                        petBattlePet.species = attribute.Value;
                        break;
                    case "next_tier_item_id":
                        petBattlePet.nextTierItemId = int.Parse(attribute.Value);
                        break;
                    case "next_tier_battle_points_needed":
                        petBattlePet.nextTierBattlePointsNeeded = int.Parse(attribute.Value);
                        break;
                    case "speed":
                        petBattlePet.speed = (PetBattlePetSpeed)(int)Enum.Parse(typeof(PetBattlePetSpeed), attribute.Value);
                        break;
                }
            }

            var list = new List<PetBattlePetAbility>();

            foreach (XmlElement item2 in item.GetElementsByTagName("abilities"))
            {
                var petBattlePetAbility = new PetBattlePetAbility();

                foreach (XmlAttribute attribute2 in item2.Attributes)
                {
                    switch (attribute2.Name)
                    {
                        case "id":
                            petBattlePetAbility.abilityId = int.Parse(attribute2.Value);
                            break;
                        case "description":
                            petBattlePetAbility.description = attribute2.Value;
                            break;
                        case "abilityIcon":
                            petBattlePetAbility.abilityIcon = attribute2.Value;

                            if (!string.IsNullOrEmpty(petBattlePetAbility.abilityIcon))
                            {
                                var separator = new string[1] { "ICO" };
                                var array = petBattlePetAbility.abilityIcon.Split(separator, StringSplitOptions.None);
                                if (array.Length > 0)
                                    petBattlePetAbility.abilityIcon = "PF" + array[1];
                            }
                            break;
                        case "index":
                            petBattlePetAbility.index = int.Parse(attribute2.Value);
                            break;
                        case "cooldownRounds":
                            petBattlePetAbility.cooldownRounds = int.Parse(attribute2.Value);
                            break;
                        case "damageValue":
                            petBattlePetAbility.damageValue = int.Parse(attribute2.Value);
                            break;
                        case "durationRounds":
                            petBattlePetAbility.durationRounds = int.Parse(attribute2.Value);
                            break;
                        case "healthValue":
                            petBattlePetAbility.healthValue = int.Parse(attribute2.Value);
                            break;
                        case "displayName":
                            petBattlePetAbility.abilityDisplayName = attribute2.Value;
                            break;
                        case "abilityTarget":
                            petBattlePetAbility.abilityTarget = attribute2.Value;
                            break;
                        case "abilityTargetId":
                            petBattlePetAbility.abilityTargetId = int.Parse(attribute2.Value);
                            break;
                        case "abilityType":
                            petBattlePetAbility.abilityType = attribute2.Value;
                            break;
                        case "abilityTypeId":
                            petBattlePetAbility.abilityTypeId = int.Parse(attribute2.Value);
                            break;
                        case "healthMitigation":
                            petBattlePetAbility.healthMitigation = int.Parse(attribute2.Value);
                            break;
                    }
                }

                list.Add(petBattlePetAbility);
            }

            petBattlePet.abilities = list;
            PetBattlePetList.Add(petBattlePet);
            PetBattlePetsDictionary.TryAdd(petBattlePet.itemId, petBattlePet);
        }
    }

    public void FinalizeBundle() => 
        GameFlow.PetBattlePetsXML = this;

    public new List<PetBattlePet> GetPetEvolutionFamily(int itemId)
    {
        if (!PetBattlePetsDictionary.TryGetValue(itemId, out var value))
        {
            Logger.LogWarning($"Item with id {itemId} does not exist in the PetBattlePetsDictionary");
            return null;
        }
        var list = new List<PetBattlePet>();
        var petBattlePet = value;
        for (var i = 0; i < PetBattlePetList.Count; i++)
        {
            var itemId2 = PetBattlePetList[i].itemId;
            var petBattlePet2 = PetBattlePetsDictionary[itemId2];
            if (petBattlePet2.species == petBattlePet.species)
                list.Add(petBattlePet2);
        }
        return list;
    }
}
