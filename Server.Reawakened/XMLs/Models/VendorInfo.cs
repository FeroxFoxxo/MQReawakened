using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.XMLs.Models;

public class VendorInfo(int gameObjectId, int nameId, int descriptionId,
    int numberOfIdolsToAccessBackStore, int idolLevelId,
    int vendorId, int catalogId, NPCController.NPCStatus vendorType,
    Conversation greetingConversation, Conversation leavingConversation, int[] items)
{
    public int GameObjectId { get; } = gameObjectId;
    public int NameId { get; set; } = nameId;
    public int DescriptionId { get; } = descriptionId;

    public int NumberOfIdolsToAccessBackStore { get; } = numberOfIdolsToAccessBackStore;
    public int IdolLevelId { get; } = idolLevelId;

    public int VendorId { get; set; } = vendorId;
    public int CatalogId { get; set; } = catalogId;

    public NPCController.NPCStatus VendorType { get; } = vendorType;

    public Conversation GreetingConversation { get; } = greetingConversation;
    public Conversation LeavingConversation { get; } = leavingConversation;

    public int[] Items { get; } = items;

    public string ToString(Player player)
    {
        var sb = new SeparatedStringBuilder('%');

        sb.Append(GameObjectId);
        sb.Append(NameId);
        sb.Append(NumberOfIdolsToAccessBackStore);

        var idolCount = 0;

        if (player.Character.CollectedIdols.TryGetValue(player.Room.LevelInfo.LevelId, out var idols))
            idolCount = idols.Count;

        sb.Append(idolCount);

        sb.Append(VendorId);
        sb.Append(CatalogId);

        sb.Append(string.Empty);

        sb.Append(GreetingConversation);
        sb.Append(LeavingConversation);

        return sb.ToString();
    }
}
