using A2m.Server;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger.Abstractions;

public abstract class BaseTriggerStatueComp<T> : BaseTriggerCoopController<T> where T : TriggerStatue
{
    public int[] TriggeredRewards;

    public int Target09LevelEditorId => ComponentData.Target09LevelEditorID;
    public int Target10LevelEditorId => ComponentData.Target10LevelEditorID;
    public int Target11LevelEditorId => ComponentData.Target11LevelEditorID;
    public int Target12LevelEditorId => ComponentData.Target12LevelEditorID;

    public int TargetReward01LevelEditorID => ComponentData.TargetReward01LevelEditorID;
    public int TargetReward02LevelEditorID => ComponentData.TargetReward02LevelEditorID;
    public int TargetReward03LevelEditorID => ComponentData.TargetReward03LevelEditorID;
    public int TargetReward04LevelEditorID => ComponentData.TargetReward04LevelEditorID;

    public enum ArenaStatus
    {
        Win,
        Lose,
        Incomplete,
        Complete
    }

    public ArenaStatus Status;
    public bool HasStarted;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        AddToTriggers(
        [
            Target09LevelEditorId,
            Target10LevelEditorId,
            Target11LevelEditorId,
            Target12LevelEditorId
        ], TriggerType.Activate);

        TriggeredRewards = [
            TargetReward01LevelEditorID,
            TargetReward02LevelEditorID,
            TargetReward03LevelEditorID,
            TargetReward04LevelEditorID
        ];

        HasStarted = false;
        Status = ArenaStatus.Incomplete;
    }

    public override object[] GetInitData(Player player) => [-1];

    public override void SendDelayedData(Player player)
    {
        var trigger = new Trigger_SyncEvent(Id.ToString(), Room.Time, false, "now", false);

        if (Status == ArenaStatus.Complete)
            trigger = new Trigger_SyncEvent(Id.ToString(), Room.Time, true, "now", false);
        else if (Status != ArenaStatus.Complete && HasStarted)
            trigger = new Trigger_SyncEvent(Id.ToString(), Room.Time, true, "now", true);

        player.SendSyncEventToPlayer(trigger);
    }

    public override void Update()
    {
        Status = GetArenaStatus();
        if (Status == ArenaStatus.Win)
            ArenaSuccess();
        else if (Status == ArenaStatus.Lose)
            ArenaFailure();
    }

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {

        if (IsActive)
        {
            StartArena();

            var players = Room.GetPlayers();
            foreach (var gamer in players)
                gamer.TempData.CurrentArena = this;
        }
        else
        {
            var players = Room.GetPlayers();

            //Trigger rewarded entities on win and shut down Arena
            if (GetArenaStatus() == ArenaStatus.Win)
            {
                foreach (var entity in TriggeredRewards)
                    foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(entity.ToString()))
                        trigger.Trigger(true, player.GameObjectId);

                foreach (var gamer in players)
                {
                    gamer.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, QuestCatalog);
                    gamer.Character.Write.SpawnPointId = Id;
                }
            }
            else
                foreach (var gamer in players)
                    RemovePhysicalInteractor(gamer, gamer.GameObjectId);
        }

        HasStarted = isActive;
    }

    public virtual ArenaStatus GetArenaStatus() => ArenaStatus.Incomplete;

    public virtual void StartArena()
    {
    }

    public virtual void ArenaSuccess()
    {
        var players = Room.GetPlayers();
        Trigger(players.FirstOrDefault(), true, false);
        foreach (var player in players)
            player.TempData.CurrentArena = null;

        Status = ArenaStatus.Complete;
    }

    public virtual void ArenaFailure()
    {
        var players = Room.GetPlayers();
        Trigger(players.FirstOrDefault(), false, false);
        foreach (var player in players)
            player.TempData.CurrentArena = null;

        Status = ArenaStatus.Incomplete;
        HasStarted = false;
    }

}
