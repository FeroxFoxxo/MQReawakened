namespace Server.Colliders.DTOs;

public record ColliderDto(
    string Id,
    string Type,
    string Plane,
    bool Active,
    bool Invisible,
    float X,
    float Y,
    float Width,
    float Height);

public record RoomCollidersDto(
    int LevelId,
    int RoomInstanceId,
    string Name,
    ColliderDto[] Colliders);

public record ColliderDiffResult(
    int LevelId,
    int RoomInstanceId,
    long Version,
    ColliderDto[] Added,
    string[] Removed,
    ColliderDto[] Updated,
    ColliderBoundsDto Bounds,
    ColliderStatsDto Stats);

public record ColliderBoundsDto(float MinX,float MinY,float MaxX,float MaxY,float Width,float Height);

public record ColliderStatsDto(int Added,int Removed,int Updated);
