using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.World;
public class ClosestEntity : SlashCommand
{
    public override string CommandName => "/closestentity";

    public override string CommandDescription => "Gets the cloesest entities near you.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public ServerRConfig ServerRConfig { get; set; }

    public override void Execute(Player player, string[] args)
    {
        var plane = player.GetPlaneEntities();

        var closestGameObjects = plane.Select(gameObject =>
        {
            var x = gameObject.ObjectInfo.Position.X - player.TempData.Position.x;
            var y = gameObject.ObjectInfo.Position.Y - player.TempData.Position.y;

            var distance = Math.Round(Math.Sqrt(Math.Pow(Math.Abs(x), 2) + Math.Pow(Math.Abs(y), 2)));

            return new Tuple<double, GameObjectModel>(distance, gameObject);
        }).OrderBy(x => x.Item1).ToList();

        if (closestGameObjects.Count == 0)
        {
            Log("No game objects found close to _player!", player);
            return;
        }

        Log("Closest Game Objects:", player);
        var count = 0;

        if (closestGameObjects.Count > ServerRConfig.MaximumEntitiesToReturnLog)
            closestGameObjects = [.. closestGameObjects.Take(ServerRConfig.MaximumEntitiesToReturnLog)];
        closestGameObjects.Reverse();

        foreach (var item in closestGameObjects)
        {
            if (count > ServerRConfig.MaximumEntitiesToReturnLog)
                break;

            Log($"{item.Item1} units: " +
                $"{item.Item2.ObjectInfo.PrefabName} " +
                $"({item.Item2.ObjectInfo.ObjectId})",
                player);

            count++;
        }
    }
}
