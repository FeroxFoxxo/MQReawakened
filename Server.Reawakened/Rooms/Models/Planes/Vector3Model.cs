using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Planes;

public class Vector3Model
{
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }

    private readonly string _id;
    private readonly Room _room;

    public Vector3Model(float x, float y, float z, string id, Room room)
    {
        SetPosition(x, y, z, true);
        _id = id;
        _room = room;
    }

    public void SetPosition(Vector3 position) => SetPosition(position.x, position.y, position.z);
    public void SetPosition(vector3 position) => SetPosition(position.x, position.y, position.z);

    public void SetPosition(float x, float y, float z, bool init = false)
    {
        X = x;
        Y = y;
        Z = z;

        if (init)
            return;

        if (_room == null)
            return;

        var collider = _room.GetColliderById(_id);

        if (collider == null)
            return;

        collider.Position = new Vector3(x, y, z);
    }

    public void SetPositionViaPlane(string parentPlane, string prefabName, Microsoft.Extensions.Logging.ILogger logger)
    {
        switch (parentPlane)
        {
            case "Plane1":
                Z = 20;
                break;
            case "Plane0":
                Z = 0;
                break;
            default:
                logger.LogError("Unknown plane: '{Plane}' for prefab {Name}", parentPlane, prefabName);
                break;
        }
    }

    public static float Distance(Vector3Model left, Vector3Model right) =>
        Mathf.Sqrt(Mathf.Pow(left.X - right.X, 2) + Mathf.Pow(left.Y - right.Y, 2) + Mathf.Pow(left.Z - right.Z, 2));

    public vector3 ToVector3() => new(X, Y, Z);
    public Vector3 ToUnityVector3() => new(X, Y, Z);

    public override string ToString() => $"({X}, {Y}, {Z})";
}
