namespace Server.Reawakened.Rooms.Models.Planes;

public class ComponentModel
{
    public Dictionary<string, string> ComponentAttributes { get; set; }

    public ComponentModel() =>
        ComponentAttributes = new Dictionary<string, string>();
}
