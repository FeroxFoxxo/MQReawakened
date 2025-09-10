using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Planes;

public class Vector3Model
{
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }

    public Vector3Model(float x, float y, float z) => SetPosition(x, y, z);

    public void SetPosition(Vector3Model position) => SetPosition(position.X, position.Y, position.Z);
    public void SetPosition(Vector3 position) => SetPosition(position.x, position.y, position.z);
    public void SetPosition(vector3 position) => SetPosition(position.x, position.y, position.z);

    public void SetPosition(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
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

    public vector3 ToVector3() => new(X, Y, Z);
    public Vector3 ToUnityVector3() => new(X, Y, Z);

    public override string ToString() => $"({X}, {Y}, {Z})";
}
