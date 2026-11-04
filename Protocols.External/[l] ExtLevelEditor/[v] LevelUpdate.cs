using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Entities.Components.GameObjects.NPC;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;
using Web.Apps.Leaderboards.Database.Scores;
using Web.Apps.Leaderboards.Enums;

namespace Protocols.External._l__ExtLevelEditor;

public class RoomUpdate : ExternalProtocol
{
    public override string ProtocolName => "lv";

    public MQRSlashCommands MQRSlashCommands { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public PetAbilities PetAbilities { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<RoomUpdate> Logger { get; set; }
    public InternalLeaderboards Leaderboards { get; set; }
    public TopScoresHandler TopScoresHandler { get; set; }

    public override void Run(string[] message)
    {
        var gameObjectStore = GetGameObjectStore(Player.Room);

        SendXt("lv", 0, gameObjectStore);

        foreach (var entityComponent in Player.Room.GetEntitiesFromType<BaseComponent>())
            entityComponent.SendDelayedData(Player);

        foreach (var enemy in Player.Room.GetEnemies())
            enemy.SendAiData(Player, Player.Room.GetPlayers().Length > 1);

        Player.TempData.CurrentArena = null;

        if (Player.TempData.FirstLogin)
        {
            // Tribe selection reminder protocol
            if (Player.Character.Allegiance == TribeType.Invalid
                && Player.Character.CompletedQuests.Contains(939))
                SendXt("dt");

            MQRSlashCommands.DisplayHelp(Player);

            Player.UpdateTribeProgression();

            // Add existing scores to leaderboards
            UpdateLeaderboards();
            
            // Fix daily quest bugs
            if (Player.Character.ActiveQuestId != 0 &&
                Player.Character.QuestLog.All(q => q.Id != Player.Character.ActiveQuestId))
                Player.Character.Write.ActiveQuestId = 0;
            
            Player.TempData.FirstLogin = false;
        }
        else
        {
            var levelInfo = Player.Room.LevelInfo;

            Player.CheckAchievement(AchConditionType.ExploreTrail,
                [levelInfo.Name], InternalAchievement, Logger);
            Player.DiscoverTribe(levelInfo.Tribe);
        }

        Player.Room.SendCharacterInfo(Player);

        //Check for player life
        if (Player.Character.CurrentLife < 1)
        {
            Player.Character.SetHealthStat(ItemCatalog);
            Player.Room.SendSyncEvent(new Health_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                Player.Character.CurrentLife, Player.Character.MaxLife, Player.GameObjectId.ToString()));
        }
        
        //Reapply still-active status effects
        Player.Character.StatusEffects.UpdateStatus();
        foreach (var status in Player.Character.StatusEffects.Snapshot())
        {
            Player.Room.SendSyncEvent(new StatusEffect_SyncEvent(Player.GameObjectId, Player.Room.Time,
                (int)status.Key, (int)status.Value.Value, (int)(status.Value.Expiry - DateTime.UtcNow).TotalSeconds, true, status.Value.PrefabName, true));
        }

        foreach (var npc in Player.Room.GetEntitiesFromType<NPCControllerComp>())
            npc.SendNpcInfo(Player);

        if (Player.Character.Pets.TryGetValue(Player.Character.PetItemId.ToString(), out var pet) && pet != null)
        {
            // not sure why we have this line
            //pet.ItemId = Player.Character.PetItemId.ToString();

            if (PetAbilities.PetAbilityData.TryGetValue(int.Parse(pet.ItemId), out var petAbility))
                Player.EquipPet(pet.ItemId, petAbility, ItemCatalog, WorldStatistics, ServerRConfig, ItemRConfig);
        }
    }

    private string GetGameObjectStore(Room room)
    {
        var sb = new SeparatedStringBuilder('&');

        if (ServerRConfig.GameVersion >= GameVersion.vEarly2013)
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
	
    private void UpdateLeaderboards()
    {
        foreach (var score in Player.Character.BestMinigameTimes)
        {
            var game = Leaderboards.Games.FirstOrDefault(x => x.name == score.Key);
            
            if (game == null)
                return;
            
            var gameId = game.id;
            
            var characterScores = TopScoresHandler.GetScoresFromCharacter(Player.Character.Id);

            var leaderboardScore = score.Key == "LV_CRS_Minigame_MonkeyBlast" ? score.Value : score.Value * 100;
            
            if (characterScores == null || characterScores.Count == 0 || characterScores.All(x => x.GameId != gameId))
            {
                TopScoresHandler.Create(gameId, (int)leaderboardScore, 0, DateTime.UtcNow, Player.Character.Id, ScoreType.AllTime);
                continue;
            }

            var characterScore = characterScores
                .FirstOrDefault(x => x.CharacterId == Player.Character.Id);

            if (characterScore != null)
                continue;
            
            TopScoresHandler.Create(gameId, (int)leaderboardScore, 0, DateTime.UtcNow, Player.Character.Id, ScoreType.AllTime);
        }
    }
}
