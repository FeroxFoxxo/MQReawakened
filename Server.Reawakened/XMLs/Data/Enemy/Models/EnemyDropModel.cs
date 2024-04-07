using Server.Reawakened.XMLs.Data.LootRewards.Enums;

public class EnemyDropModel(DynamicDropType type, int id, float chance, int minLevel, int maxLevel)
{
    public DynamicDropType Type = type;
    public int Id = id;
    public float Chance = chance;
    public int MinLevel = minLevel;
    public int MaxLevel = maxLevel;
}
