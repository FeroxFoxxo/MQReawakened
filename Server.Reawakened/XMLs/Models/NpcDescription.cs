namespace Server.Reawakened.XMLs.Models;

public class NpcDescription
{
    public readonly int NameTextId;
    public readonly int ObjectId;
    public readonly NPCController.NPCStatus Status;

    public NpcDescription(int objectId, int nameTextId, NPCController.NPCStatus status)
    {
        ObjectId = objectId;
        NameTextId = nameTextId;
        Status = status;
    }
}
