using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
public class AISyncEventHelper
{
    public static AIInit_SyncEvent AIInit(string id, float time, float posX, float posY, float posZ, float spawnX,
        float spawnY, float behaviorRatio, int health, int maxHealth, float healthMod, float scaleMod, float resMod,
        int stars, int level, GlobalProperties globalProperties, Dictionary<StateType, BaseState> states,
        AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp)
    {
        var bList = new SeparatedStringBuilder('`');

        foreach (var behavior in states)
        {
            var bDefinesList = new SeparatedStringBuilder('|');

            bDefinesList.Append(Enum.GetName(behavior.Key));
            bDefinesList.Append(behavior.Value.ToStateString(globalComp, genericComp));
            bDefinesList.Append(behavior.Value.ToResourcesString());

            bList.Append(bDefinesList.ToString());
        }

        var behaviors = bList.ToString();

        var aiInit = new AIInit_SyncEvent(
            id,
            time,
            posX,
            posY,
            posZ,
            spawnX,
            spawnX,
            behaviorRatio,
            health, 
            maxHealth,
            healthMod, 
            scaleMod, 
            resMod, 
            stars, 
            level,
            globalProperties.ToString(),
            behaviors
        );

        aiInit.EventDataList[2] = spawnX;
        aiInit.EventDataList[3] = spawnY;
        aiInit.EventDataList[4] = posZ;

        return aiInit;
    }

    public static AIDo_SyncEvent AIDo(string id, float time, float posX, float posY, float speedFactor, int behaviorId, string[] inArgs, float targetPosX, float targetPosY, int direction, bool awareBool)
    {
        var argBuilder = new SeparatedStringBuilder('`');

        foreach (var str in inArgs)
            argBuilder.Append(str);

        var args = argBuilder.ToString();

        var aiDo = new AIDo_SyncEvent(new SyncEvent(id, SyncEvent.EventType.AIDo, time));

        aiDo.EventDataList.Clear();
        aiDo.EventDataList.Add(posX);
        aiDo.EventDataList.Add(posY);
        aiDo.EventDataList.Add(speedFactor);
        aiDo.EventDataList.Add(behaviorId);
        aiDo.EventDataList.Add(args);
        aiDo.EventDataList.Add(targetPosX);
        aiDo.EventDataList.Add(targetPosY);
        aiDo.EventDataList.Add(direction);
        aiDo.EventDataList.Add(awareBool ? 1 : 0);

        return aiDo;
    }

    public static AILaunchItem_SyncEvent AILaunchItem(string id, float time, float posX, float posY, float posZ, float speedX, float speedY, float lifeTime, int prjId, bool isGrenade)
    {
        var launch = new AILaunchItem_SyncEvent(new SyncEvent(id, SyncEvent.EventType.AILaunchItem, time));

        launch.EventDataList.Clear();
        launch.EventDataList.Add(posX);
        launch.EventDataList.Add(posY);
        launch.EventDataList.Add(posZ);
        launch.EventDataList.Add(speedX);
        launch.EventDataList.Add(speedY);
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
        new (false, 0, 0, 0, 0, 0, 0, 0, 0, 0, "Generic", string.Empty, false, false, 0);

    public static GenericScriptPropertiesModel CreateDefaultGenericScript() =>
        new (StateType.Unknown, StateType.Unknown, StateType.Unknown, 0, 0, 0);
}
