using A2m.Server;

namespace Server.Reawakened.Players.Models.Character;

public class TemporaryEquipmentModel
{
    public Dictionary<ItemSubCategory, int> EquippedItems { get; set; }
    public List<ItemSubCategory> EquippedBinding { get; set; }

    public TemporaryEquipmentModel(string serverString)
    {
        EquippedItems = [];
        EquippedBinding = [];

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
}
