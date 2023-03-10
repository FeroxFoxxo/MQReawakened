using Microsoft.Extensions.Logging;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.Entities;
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

        foreach (var entity in Player.Room.Entities.Values.SelectMany(x => x))
            entity.SendDelayedData(Player);

        Player.Room.SendCharacterInfo(Player);

        foreach (var npc in Player.Room.GetEntities<NpcControllerEntity>())
            npc.Value.SendNpcInfo(Player.Character, NetState);

        if (!Player.FirstLogin)
            return;

        ChatCommands.DisplayHelp(Player);
        Player.FirstLogin = false;
    }

    private string GetGameObjectStore(Dictionary<int, List<BaseSyncedEntity>> entities)
    {
        var sb = new SeparatedStringBuilder('&');

        foreach (var gameObject in entities.Select(GetGameObject)
                     .Where(gameObject => gameObject.Split('~').Length > 1))
            sb.Append(gameObject);

        return sb.ToString();
    }

    private string GetGameObject(KeyValuePair<int, List<BaseSyncedEntity>> entities)
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append(entities.Key);

        foreach (var entity in entities.Value)
            sb.Append(GetComponent(entity));

        return sb.ToString();
    }

    private string GetComponent(BaseSyncedEntity entity)
    {
        var sb = new SeparatedStringBuilder('~');

        sb.Append(entity.Name);

        foreach (var setting in entity.GetInitData(Player))
            sb.Append(setting);

        return sb.ToString();
    }
}
