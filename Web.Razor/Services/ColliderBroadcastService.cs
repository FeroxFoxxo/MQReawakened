using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Rooms.Services;
using Web.Razor.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Web.Razor.Services;

public class ColliderBroadcastService(WorldHandler _world, IHubContext<ColliderHub> _hub, ILogger<ColliderBroadcastService> _logger) : BackgroundService, IService
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

                    var colliderData = room.GetColliders().Select(c => new
                    {
                        id = c.Id,
                        type = c.Type.ToString(),
                        plane = c.Plane,
                        active = c.Active,
                        invisible = c.IsInvisible,
                        c.Position.x,
                        c.Position.y,
                        width = (float)c.BoundingBox.width,
                        height = (float)c.BoundingBox.height
                    }).ToArray();

                    var payload = new
                    {
                        levelId,
                        roomInstanceId,
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
