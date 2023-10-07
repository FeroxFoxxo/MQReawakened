using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.XMLs.Models;

public class VendorInfo(int gameObjectId, int nameId, int descriptionId,
    int numberOfIdolsToAccessBackStore, int idolLevelId,
    int vendorId, int catalogId, NPCController.NPCStatus vendorType,
    Conversation greetingConversation, Conversation leavingConversation)
{
    public readonly int GameObjectId = gameObjectId;
    public readonly int NameId = nameId;
    public readonly int DescriptionId = descriptionId;

    public readonly int NumberOfIdolsToAccessBackStore = numberOfIdolsToAccessBackStore;
    public readonly int IdolLevelId = idolLevelId;

    public readonly int VendorId = vendorId;
    public readonly int CatalogId = catalogId;
    public readonly NPCController.NPCStatus VendorType = vendorType;

    public readonly Conversation GreetingConversation = greetingConversation;
    public readonly Conversation LeavingConversation = leavingConversation;

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

        sb.Append(GreetingConversation);
        sb.Append(LeavingConversation);

        return sb.ToString();
    }
}
