using UnityEngine;

public class ColliderModel(string plane, float x, float y, float sx, float sy)
{
    public string Plane => plane;
    public Vector2 Position => new(x, y);
    public float Width => sx;
    public float Height => sy;
}
