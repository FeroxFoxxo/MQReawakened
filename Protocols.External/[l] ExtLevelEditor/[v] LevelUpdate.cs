using Microsoft.Extensions.Logging;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities;

namespace Protocols.External._l__ExtLevelEditor;

public class RoomUpdate : ExternalProtocol
{
    public override string ProtocolName => "lv";

    public ILogger<RoomUpdate> Logger { get; set; }
    public ChatCommands ChatCommands { get; set; }
    public ServerRConfig Config { get; set; }

    public override void Run(string[] message)
    {
        var gameObjectStore = GetGameObjectStore(Player.Room);

        SendXt("lv", 0, gameObjectStore);

        foreach (var entityComponent in Player.Room.GetEntitiesFromType<BaseComponent>())
            entityComponent.SendDelayedData(Player);

        foreach (var enemy in Player.Room.Enemies.Values)
            enemy.SendInitData(Player);

        Player.Room.SendCharacterInfo(Player);

        foreach (var npc in Player.Room.GetEntitiesFromType<NPCControllerComp>())
            npc.SendNpcInfo(Player);

        if (!Player.FirstLogin)
            return;

        ChatCommands.DisplayHelp(Player);
        Player.FirstLogin = false;
    }

    private string GetGameObjectStore(Room room)
    {
        var sb = new SeparatedStringBuilder('&');

        if (Config.GameVersion >= GameVersion.vEarly2013)
        {
            foreach (var gameObject in room.GetEntities().Where(e => e.Value != null).Select(GetEntity)
                         .Where(gameObject => gameObject.Split('~').Length > 1))
                if (!string.IsNullOrEmpty(gameObject))
                    sb.Append(gameObject);
        }
        else
        {
            foreach (var gameObject in room.GetEntities().Where(e => e.Value != null))
            {
                var entityId = gameObject.Key;
                var entityComponents = gameObject.Value;

                if (entityComponents == null)
                    continue;

                var componentData = entityComponents.Select(x => x.GetInitData(Player))
                    .Where(x => x != null)
                    .Where(x => x.Length > 0)
                    .ToArray();

                if (componentData.Length <= 0)
                    continue;

                if (componentData.Length > 1)
                    Logger.LogError("Too many components for {EntityId}!", entityId);

                var sbEntity = new SeparatedStringBuilder('|');

                sbEntity.Append(entityId);

                var component = componentData.FirstOrDefault();

                sbEntity.Append(string.Join('!', component.Select(x => x.ToString())));

                sb.Append(sbEntity.ToString());
            }
        }

        return sb.ToString();
    }

    private string GetEntity(KeyValuePair<string, List<BaseComponent>> entity)
    {
        var entityId = entity.Key;
        var entityComponents = entity.Value;

        var sb = new SeparatedStringBuilder('|');

        sb.Append(entityId);

        if (entityComponents != null)
            foreach (var component in entityComponents)
                if (component != null)
                    sb.Append(GetComponent(component));

        return sb.ToString();
    }

    private string GetComponent(BaseComponent component)
    {
        var sb = new SeparatedStringBuilder('~');

        sb.Append(component.Name);

        foreach (var setting in component.GetInitData(Player))
            if (setting != null)
                sb.Append(setting);

        return sb.ToString();
    }
}
