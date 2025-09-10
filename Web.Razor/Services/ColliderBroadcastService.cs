using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Rooms.Services;
using Web.Razor.Hubs;
using Microsoft.AspNetCore.SignalR;
using Server.Colliders.Services;

namespace Web.Razor.Services;

public class ColliderBroadcastService(WorldHandler _world, IHubContext<ColliderHub> _hub, ILogger<ColliderBroadcastService> _logger, InMemoryRoomVersionTracker _versions) : BackgroundService, IService
{
    public void Initialize() { }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var room in _world.GetOpenRooms())
                {
                    var levelId = room.LevelInfo.LevelId;
                    var roomInstanceId = int.Parse(room.ToString().Split('_').Last());
                    var group = $"room:{levelId}:{roomInstanceId}";

                    var colliderDtos = ColliderHelper.BuildCollidersForRoom(room);

                    var colliderData = colliderDtos.Select(c => new
                    {
                        id = c.Id,
                        type = c.Type,
                        plane = c.Plane,
                        active = c.Active,
                        invisible = c.Invisible,
                        x = c.X,
                        y = c.Y,
                        width = c.Width,
                        height = c.Height
                    }).ToArray();

                    var version = _versions?.Get(levelId, roomInstanceId) ?? 0L;
                    var payload = new
                    {
                        levelId,
                        roomInstanceId,
                        version,
                        colliders = colliderData
                    };

                    await _hub.Clients.Group(group).SendAsync("collidersUpdated", payload, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting collider updates");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
