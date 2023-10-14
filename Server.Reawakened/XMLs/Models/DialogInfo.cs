namespace Server.Reawakened.XMLs.Models;

public class DialogInfo(int gameObjectId, int nameId, int descriptionId, Dictionary<int, Conversation> dialog)
{
    public int GameObjectId { get; } = gameObjectId;
    public int NameId { get; set; } = nameId;
    public int DescriptionId { get; } = descriptionId;
    public Dictionary<int, Conversation> Dialog { get; } = dialog;
}
