using UnityEngine;

namespace Server.Reawakened.XMLs.Data.Collider;

public class ColliderModel(string plane, float x, float y, float sx, float sy)
{
    public string Plane => plane;
    public float Width => sx;
    public float Height => sy;

    public Vector3 Position = new(x, y, plane == "Plane0" ? 0 : 20);
}
