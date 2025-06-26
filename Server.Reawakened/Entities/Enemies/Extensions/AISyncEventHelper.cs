using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.Extensions;
public static class AISyncEventHelper
{
    public static AIInit_SyncEvent AIInit(float posX, float posY, float posZ, float spawnX, float spawnY, float behaviorRatio, BehaviorEnemy behaviorEnemy) =>
        AIInit(
            behaviorEnemy.Id, behaviorEnemy.Room,
            posX, posY, posZ, spawnX, spawnY, behaviorRatio,
            behaviorEnemy.Health, behaviorEnemy.MaxHealth,
            behaviorEnemy.HealthModifier, behaviorEnemy.ScaleModifier, behaviorEnemy.ResistanceModifier,
            behaviorEnemy.Status.Stars, behaviorEnemy.Level, behaviorEnemy.Global.GetGlobalProperties(), behaviorEnemy.EnemyModel.BehaviorData, behaviorEnemy.Behaviors, behaviorEnemy
        );

    public static AIInit_SyncEvent AIInit(
        string id, Room room,
        float posX, float posY, float posZ, float spawnX, float spawnY, float behaviorRatio,
        int health, int maxHealth, float healthModifier, float scaleModifier, float resistanceModifier,
        int stars, int level, GlobalProperties globalProperties, Dictionary<StateType, BaseState> states,
        Dictionary<StateType, AIBaseBehavior> behaviors = null, BehaviorEnemy enemy = null
    )
    {
        behaviors ??= states.ToDictionary(s => s.Key, s => s.Value.GetBaseBehaviour(enemy));

        var bList = new SeparatedStringBuilder('`');

        foreach (var behavior in states)
        {
            var bDefinesList = new SeparatedStringBuilder('|');

            bDefinesList.Append(Enum.GetName(behavior.Key));

            bDefinesList.Append(behaviors[behavior.Key].GetProperties());
            bDefinesList.Append(behavior.Value.ToResourcesString());

            bList.Append(bDefinesList.ToString());
        }

        var aiInit = new AIInit_SyncEvent(
            id, room.Time,
            posX, posY, posZ, spawnX, spawnY, behaviorRatio,
            health, maxHealth, healthModifier, scaleModifier, resistanceModifier,
            stars, level,
            globalProperties.ToString(),
            bList.ToString()
        );

        aiInit.EventDataList[2] = spawnX;
        aiInit.EventDataList[3] = spawnY;
        aiInit.EventDataList[4] = posZ;

        return aiInit;
    }

    public static AIDo_SyncEvent AIDo(float posX, float posY, float speedFactor, float targetPosX, float targetPosY, int direction, bool awareBool, BehaviorEnemy enemy) =>
        AIDo(enemy.Id, enemy.Room, posX, posY, speedFactor, targetPosX, targetPosY, direction, awareBool, IndexOf(enemy.CurrentState, enemy.EnemyModel.BehaviorData), enemy.CurrentBehavior.GetStartArgsString());

    public static AIDo_SyncEvent AIDo(string id, Room room, float posX, float posY, float speedFactor, float targetPosX, float targetPosY, int direction, bool awareBool, int index, string startString)
    {
        var aiDo = new AIDo_SyncEvent(new SyncEvent(id, SyncEvent.EventType.AIDo, room.Time));

        aiDo.EventDataList.Clear();
        aiDo.EventDataList.Add(posX);
        aiDo.EventDataList.Add(posY);
        aiDo.EventDataList.Add(speedFactor);
        aiDo.EventDataList.Add(index);
        aiDo.EventDataList.Add(startString);
        aiDo.EventDataList.Add(targetPosX);
        aiDo.EventDataList.Add(targetPosY);
        aiDo.EventDataList.Add(direction);
        aiDo.EventDataList.Add(awareBool ? 1 : 0);

        return aiDo;
    }

    public static int IndexOf(StateType behaviorType, Dictionary<StateType, BaseState> states)
    {
        var index = 0;

        foreach (var behavior in states)
        {
            if (behavior.Key == behaviorType)
                return index;

            index++;
        }

        return 0;
    }

    public static AILaunchItem_SyncEvent AILaunchItem(string id, float time, Vector3 position, Vector2 speed, float lifeTime, int prjId, bool isGrenade)
    {
        var launch = new AILaunchItem_SyncEvent(new SyncEvent(id, SyncEvent.EventType.AILaunchItem, time));

        launch.EventDataList.Clear();
        launch.EventDataList.Add(position.x);
        launch.EventDataList.Add(position.y);
        launch.EventDataList.Add(position.z);
        launch.EventDataList.Add(speed.x);
        launch.EventDataList.Add(speed.y);
        launch.EventDataList.Add(lifeTime);
        launch.EventDataList.Add(prjId);
        launch.EventDataList.Add(isGrenade ? 1 : 0);

        return launch;
    }

    public static AIDie_SyncEvent AIDie(string id, float time, string spawnItemPrefabName, int spawnItemQuantity, bool spawnLoot, string killedById, bool suicide)
    {
        var die = new AIDie_SyncEvent(new SyncEvent(id, SyncEvent.EventType.AIDie, time));

        die.EventDataList.Add(spawnItemPrefabName);
        die.EventDataList.Add(spawnItemQuantity);
        die.EventDataList.Add(spawnLoot ? 1 : 0);
        die.EventDataList.Add(killedById);
        die.EventDataList.Add(suicide ? 1 : 0);

        return die;
    }

    public static GlobalProperties CreateDefaultGlobalProperties() =>
        new(false, 0, 0, 0, 0, 0, 0, 0, 0, 0, "Generic", string.Empty, false, false, 0);
}
