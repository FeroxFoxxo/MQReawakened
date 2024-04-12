using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.XMLs.Data.Npcs;

public class VendorInfo(int gameObjectId, int nameId, int descriptionId,
    int numberOfIdolsToAccessBackStore, int idolLevelId,
    int vendorId, int catalogId, NPCController.NPCStatus vendorType,
    ConversationInfo greetingConversation, ConversationInfo leavingConversation, int[] items)
{
    public int GameObjectId => gameObjectId;
    public int NameId { get; set; } = nameId;
    public int DescriptionId => descriptionId;

    public int NumberOfIdolsToAccessBackStore => numberOfIdolsToAccessBackStore;
    public int IdolLevelId => idolLevelId;

    public int VendorId { get; set; } = vendorId;
    public int CatalogId { get; set; } = catalogId;

    public NPCController.NPCStatus VendorType => vendorType;

    public ConversationInfo GreetingConversation => greetingConversation;
    public ConversationInfo LeavingConversation => leavingConversation;

    public int[] Items => items;

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
