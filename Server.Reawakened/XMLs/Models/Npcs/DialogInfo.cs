namespace Server.Reawakened.XMLs.Models.Npcs;

public class DialogInfo(int gameObjectId, int nameId, int descriptionId, Dictionary<int, ConversationInfo> dialog)
{
    public int GameObjectId { get; } = gameObjectId;
    public int NameId { get; set; } = nameId;
    public int DescriptionId { get; } = descriptionId;
    public Dictionary<int, ConversationInfo> Dialog { get; } = dialog;
}
