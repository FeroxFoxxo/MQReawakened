using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity.Utils;
public class AISyncEventHelper
{
    public static AIDo_SyncEvent AIDo(BaseComponent entity, Vector3 position, float speedFactor, int behaviorId, string args, float targetPosX, float targetPosY, int direction, bool awareBool)
    {
        var aiDo = new AIDo_SyncEvent(new SyncEvent(entity.Id, SyncEvent.EventType.AIDo, entity.Room.Time));
        aiDo.EventDataList.Clear();
        aiDo.EventDataList.Add(position.x);
        aiDo.EventDataList.Add(position.y);
        aiDo.EventDataList.Add(speedFactor);
        aiDo.EventDataList.Add(behaviorId);
        aiDo.EventDataList.Add(args);
        aiDo.EventDataList.Add(targetPosX);
        aiDo.EventDataList.Add(targetPosY);
        aiDo.EventDataList.Add(direction);
        aiDo.EventDataList.Add(awareBool ? 1 : 0);

        return aiDo;
    }

    public static AILaunchItem_SyncEvent AILaunchItem(BaseComponent entity, float posX, float posY, float posZ, float speedX, float speedY, float lifeTime, int prjId, int isGrenade)
    {
        var launch = new AILaunchItem_SyncEvent(new SyncEvent(entity.Id, SyncEvent.EventType.AILaunchItem, entity.Room.Time));
        launch.EventDataList.Clear();
        launch.EventDataList.Add(posX);
        launch.EventDataList.Add(posY);
        launch.EventDataList.Add(posZ);
        launch.EventDataList.Add(speedX);
        launch.EventDataList.Add(speedY);
        launch.EventDataList.Add(lifeTime);
        launch.EventDataList.Add(prjId);
        launch.EventDataList.Add(isGrenade);

        return launch;
    }

    public static AIDie_SyncEvent AIDie(BaseComponent entity, string spawnItemPrefabName, int spawnItemQuantity, bool spawnLoot, string killedById, bool suicide)
    {
        var die = new AIDie_SyncEvent(new SyncEvent(entity.Id, SyncEvent.EventType.AIDie, entity.Room.Time));
        die.EventDataList.Add(spawnItemPrefabName);
        die.EventDataList.Add(spawnItemQuantity);
        die.EventDataList.Add(spawnLoot ? 1 : 0);
        die.EventDataList.Add(killedById);
        die.EventDataList.Add(suicide ? 1 : 0);

        return die;
    }
}
