using Microsoft.Extensions.Logging;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Protocols.External._l__ExtLevelEditor;

public class RoomUpdate : ExternalProtocol
{
    public override string ProtocolName => "lv";

    public ILogger<RoomUpdate> Logger { get; set; }
    public ChatCommands ChatCommands { get; set; }

    public override void Run(string[] message)
    {
        var gameObjectStore = GetGameObjectStore(Player.Room.Entities);

        SendXt("lv", 0, gameObjectStore);

        foreach (var entityComponent in Player.Room.Entities.Values.SelectMany(x => x))
            entityComponent.SendDelayedData(Player);

        Player.Room.SendCharacterInfo(Player);

        foreach (var npc in Player.Room.GetComponentsOfType<NPCControllerComp>())
            npc.Value.SendNpcInfo(Player.Character, NetState);

        if (!Player.FirstLogin)
            return;

        ChatCommands.DisplayHelp(Player);
        Player.FirstLogin = false;
    }

    private string GetGameObjectStore(Dictionary<int, List<BaseComponent>> entities)
    {
        var sb = new SeparatedStringBuilder('&');

        foreach (var gameObject in entities.Where(e => e.Value != null).Select(GetEntity)
                     .Where(gameObject => gameObject.Split('~').Length > 1))
            if (!string.IsNullOrEmpty(gameObject))
                sb.Append(gameObject);

        return sb.ToString();
    }

    private string GetEntity(KeyValuePair<int, List<BaseComponent>> entity)
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
