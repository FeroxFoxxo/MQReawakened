using A2m.Server;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class EquipmentModel(CharacterDbEntry entry)
{
    public Dictionary<ItemSubCategory, int> EquippedItems => entry.EquippedItems;
    public List<ItemSubCategory> EquippedBinding => entry.EquippedBinding;

    public void UpdateFromTempEquip(TemporaryEquipmentModel model)
    {
        entry.EquippedItems = model.EquippedItems;
        entry.EquippedBinding = model.EquippedBinding;
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(':');

        foreach (var equipped in EquippedItems)
            sb.Append(GetEquippedItemString(equipped));

        return sb.ToString();
    }

    public string GetEquippedItemString(KeyValuePair<ItemSubCategory, int> equipped)
    {
        var sb = new SeparatedStringBuilder('=');

        sb.Append((int)equipped.Key);
        sb.Append(equipped.Value);
        sb.Append(EquippedBinding.Contains(equipped.Key) ? 1 : 0);

        return sb.ToString();
    }
}
