namespace Server.Reawakened.Rooms.Models.Planes;

public class ObjectInfoModel
{
    public Vector3Model Position { get; set; }

    public Vector3Model Rotation { get; set; }

    public Vector3Model Scale { get; set; }

    public int ObjectId { get; set; }

    public string PrefabName { get; set; }

    public string ParentPlane { get; set; }

    public RectModel Rectangle { get; set; }

    public Dictionary<string, ComponentModel> Components { get; set; }

    public ObjectInfoModel() =>
        Components = [];
}
