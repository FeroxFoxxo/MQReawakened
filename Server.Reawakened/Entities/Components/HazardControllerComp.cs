using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Entities.Components;

public class HazardControllerComp : BaseHazardControllerComp<HazardController>
{
    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
        if (notifyCollisionEvent.Colliding && HurtEffect != ItemEffectType.PoisonDamage.ToString() &&
            !PrefabName.Contains("ToxicCloud"))
        {
            player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
                (int)ItemEffectType.BluntDamage, 1, 1, true, PrefabName, false));
            player.ApplyCharacterDamage(Room, (int)Math.Ceiling(player.Character.Data.MaxLife * HealthRatioDamage), DamageDelay, TimerThread);

            Logger.LogInformation("{characterName} collided with Hazard ({Id}).", player.CharacterName, Id);
        }
    }
}
