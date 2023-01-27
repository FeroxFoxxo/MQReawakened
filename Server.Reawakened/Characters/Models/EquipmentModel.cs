using A2m.Server;
using System.Text;

namespace Server.Reawakened.Characters.Models;

public class EquipmentModel
{
    public const char KeyValueDelimiter = '=';
    public const char PropertyDelimiter = ':';

    public Dictionary<ItemSubCategory, int> EquippedItems { get; set; }
    public List<ItemSubCategory> EquippedBinding { get; set; }

    public EquipmentModel()
    {
        EquippedItems = new Dictionary<ItemSubCategory, int>();
        EquippedBinding = new List<ItemSubCategory>();
    }

    public EquipmentModel(string serverString)
    {
        EquippedItems = new Dictionary<ItemSubCategory, int>();
        EquippedBinding = new List<ItemSubCategory>();

        var items = serverString.Split(':');
        foreach (var item in items)
        {
            var values = item.Split('=');
            if (values.Length != 3)
                continue;

            var key = (ItemSubCategory)int.Parse(values[0]);
            var itemId = int.Parse(values[1]);
            var binding = int.Parse(values[2]);

            EquippedItems.Add(key, itemId);

            if (binding == 1)
                EquippedBinding.Add(key);
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var equipped in EquippedItems)
        {
            sb.Append((int)equipped.Key);
            sb.Append(KeyValueDelimiter);
            sb.Append(equipped.Value);
            sb.Append(KeyValueDelimiter);
            sb.Append(EquippedBinding.Contains(equipped.Key) ? 1 : 0);
            sb.Append(PropertyDelimiter);
        }
        return sb.ToString();
    }
}
