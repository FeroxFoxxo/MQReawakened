using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Base;
public class PetBattlePets : PetBattlePetsXML, IBundledXml
{
    public string BundleName => "PetBattlePets";
    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<PetBattlePets> Logger { get; set; }
    public ServerRConfig RConfig { get; set; }

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        if (RConfig.GameVersion >= GameVersion.vPetMasters2014)
        {
            PetBattlePetsDictionary = [];
            PetBattlePetList = [];

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            foreach (XmlElement petBattlePetXml in xmlDocument.GetElementsByTagName("petBattlePet"))
            {
                var petBattlePet = new PetBattlePet();

                foreach (XmlAttribute attribute in petBattlePetXml.Attributes)
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
                            petBattlePet.speed = Enum.Parse<PetBattlePetSpeed>(attribute.Value);
                            break;
                    }

                var abilities = new List<PetBattlePetAbility>();

                foreach (XmlElement petAbilities in petBattlePetXml.GetElementsByTagName("abilities"))
                {
                    var petBattlePetAbility = new PetBattlePetAbility();

                    foreach (XmlAttribute attribute in petAbilities.Attributes)
                        switch (attribute.Name)
                        {
                            case "id":
                                petBattlePetAbility.abilityId = int.Parse(attribute.Value);
                                break;
                            case "description":
                                petBattlePetAbility.description = attribute.Value;
                                break;
                            case "abilityIcon":
                                petBattlePetAbility.abilityIcon = attribute.Value;

                                if (!string.IsNullOrEmpty(petBattlePetAbility.abilityIcon))
                                {
                                    var separator = new string[1] { "ICO" };
                                    var array = petBattlePetAbility.abilityIcon.Split(separator, StringSplitOptions.None);

                                    if (array.Length > 0)
                                        petBattlePetAbility.abilityIcon = "PF" + array[1];
                                }
                                break;
                            case "index":
                                petBattlePetAbility.index = int.Parse(attribute.Value);
                                break;
                            case "cooldownRounds":
                                petBattlePetAbility.cooldownRounds = int.Parse(attribute.Value);
                                break;
                            case "damageValue":
                                petBattlePetAbility.damageValue = int.Parse(attribute.Value);
                                break;
                            case "durationRounds":
                                petBattlePetAbility.durationRounds = int.Parse(attribute.Value);
                                break;
                            case "healthValue":
                                petBattlePetAbility.healthValue = int.Parse(attribute.Value);
                                break;
                            case "displayName":
                                petBattlePetAbility.abilityDisplayName = attribute.Value;
                                break;
                            case "abilityTarget":
                                petBattlePetAbility.abilityTarget = attribute.Value;
                                break;
                            case "abilityTargetId":
                                petBattlePetAbility.abilityTargetId = int.Parse(attribute.Value);
                                break;
                            case "abilityType":
                                petBattlePetAbility.abilityType = attribute.Value;
                                break;
                            case "abilityTypeId":
                                petBattlePetAbility.abilityTypeId = int.Parse(attribute.Value);
                                break;
                            case "healthMitigation":
                                petBattlePetAbility.healthMitigation = int.Parse(attribute.Value);
                                break;
                        }

                    abilities.Add(petBattlePetAbility);
                }

                petBattlePet.abilities = abilities;

                PetBattlePetList.Add(petBattlePet);
                PetBattlePetsDictionary.TryAdd(petBattlePet.itemId, petBattlePet);
            }
        }
    }

    public void FinalizeBundle()
    {
        if (RConfig.GameVersion >= GameVersion.vPetMasters2014)
            GameFlow.PetBattlePetsXML = this;
    }

    public new List<PetBattlePet> GetPetEvolutionFamily(int itemId)
    {
        if (!PetBattlePetsDictionary.TryGetValue(itemId, out var value))
        {
            Logger.LogWarning("Item with id {_itemId} does not exist in the PetBattlePetsDictionary", itemId);
            return null;
        }

        var list = new List<PetBattlePet>();
        var petBattlePet = value;

        for (var i = 0; i < PetBattlePetList.Count; i++)
        {
            var petId = PetBattlePetList[i].itemId;
            var pet = PetBattlePetsDictionary[petId];

            if (pet.species == petBattlePet.species)
                list.Add(pet);
        }

        return list;
    }
}
