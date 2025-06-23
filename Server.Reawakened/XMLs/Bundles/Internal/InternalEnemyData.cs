using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.Models;
using Server.Reawakened.XMLs.Data.LootRewards.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalEnemyData : InternalXml
{
    public override string BundleName => "InternalEnemyData";
    public override BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalEnemyData> Logger { get; set; }

    public Dictionary<string, EnemyModel> EnemyInfoCatalog;

    public override void InitializeVariables() => EnemyInfoCatalog = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode enemyXml in xmlDocument.ChildNodes)
        {
            if (enemyXml.Name != "InternalEnemyData") continue;

            foreach (XmlNode enemyCategoryElement in enemyXml.ChildNodes)
            {
                if (enemyCategoryElement.Name != "EnemyCategory") continue;

                var enemyCategory = EnemyCategory.Unknown;

                foreach (XmlAttribute categoryData in enemyCategoryElement.Attributes)
                    switch (categoryData.Name)
                    {
                        case "name":
                            enemyCategory = Enum.Parse<EnemyCategory>(categoryData.Value);
                            break;
                    }

                foreach (XmlNode enemy in enemyCategoryElement.ChildNodes)
                {
                    if (enemy.Name != "Enemy") continue;

                    var prefabName = string.Empty;

                    foreach (XmlAttribute enemyData in enemy.Attributes)
                        switch (enemyData.Name)
                        {
                            case "name":
                                prefabName = enemyData.Value;
                                break;
                        }

                    var enemyModel = new EnemyModel()
                    {
                        AiType = AiType.Unknown,
                        EnemyCategory = enemyCategory
                    };

                    foreach (XmlNode data in enemy.ChildNodes)
                        switch (data.Name)
                        {
                            case "LootTable":
                                var lootTable = new List<EnemyDropModel>();

                                foreach (XmlNode dynamicDrop in data.ChildNodes)
                                {
                                    var dropType = DynamicDropType.Unknown;
                                    var dropId = 0;
                                    var dropChance = 0f;
                                    var dropMinLevel = 1;
                                    var dropMaxLevel = 65;

                                    foreach (XmlAttribute dynamicDropAttributes in dynamicDrop.Attributes)
                                        switch (dynamicDropAttributes.Name)
                                        {
                                            case "type":
                                                switch (dynamicDropAttributes.Value)
                                                {
                                                    case "item":
                                                        dropType = DynamicDropType.Item;
                                                        break;
                                                    case "randomArmor":
                                                        dropType = DynamicDropType.RandomArmor;
                                                        break;
                                                    case "randomIngredient":
                                                        dropType = DynamicDropType.RandomIngredient;
                                                        break;
                                                }
                                                break;
                                            case "id":
                                                dropId = int.Parse(dynamicDropAttributes.Value);
                                                break;
                                            case "chance":
                                                dropChance = float.Parse(dynamicDropAttributes.Value);
                                                break;
                                            case "minLevel":
                                                dropMinLevel = int.Parse(dynamicDropAttributes.Value);
                                                break;
                                            case "maxLevel":
                                                dropMaxLevel = int.Parse(dynamicDropAttributes.Value);
                                                break;
                                        }

                                    lootTable.Add(new EnemyDropModel(dropType, dropId, dropChance, dropMinLevel, dropMaxLevel));
                                }

                                enemyModel.EnemyLootTable = lootTable;
                                break;
                            default:
                                Logger.LogError("Unknown enemy data type for: {DataType} ({EnemyName}", data.Name, prefabName);
                                break;
                        }

                    enemyModel.EnsureValidData(prefabName, Logger);

                    EnemyInfoCatalog.Add(prefabName, enemyModel);
                }
            }
        }
    }
}
