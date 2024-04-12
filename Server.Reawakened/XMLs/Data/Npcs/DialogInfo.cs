namespace Server.Reawakened.XMLs.Data.Npcs;

public class DialogInfo(int gameObjectId, int nameId, int descriptionId, Dictionary<int, ConversationInfo> dialog)
{
    public int GameObjectId => gameObjectId;
    public int NameId { get; set; } = nameId;
    public int DescriptionId => descriptionId;
    public Dictionary<int, ConversationInfo> Dialog => dialog;
}
