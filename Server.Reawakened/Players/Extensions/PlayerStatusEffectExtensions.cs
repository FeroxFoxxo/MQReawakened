using A2m.Server;
using GameError;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using static Server.Reawakened.Players.Extensions.PlayerStatusEffectExtensions;


namespace Server.Reawakened.Players.Extensions;

public static class PlayerStatusEffectExtensions
{
    public static void ApplySlowEffect(this Player player, string hazardId, int damage) =>
        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.SlowStatusEffect, damage, 1, true, hazardId, false));

    //Doesn't seem to apply fast enough.
    public static void NullifySlowStatusEffect(this Player player, string hazardId) =>
        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.NullifySlowStatusEffect, 1, 1, true, hazardId, true));

    public static bool HasNullifyEffect(this Player player, ItemCatalog itemCatalog) =>
     player.Character.Equipment.EquippedItems
         .Select(x => itemCatalog.GetItemFromId(x.Value))
         .Any(item => item.ItemEffects.Any(effect => effect.Type == ItemEffectType.NullifySlowStatusEffect));

    public static void StartPoisonDamage(this Player player, string hazardId, int damage, int hurtLength, ServerRConfig serverRConfig, TimerThread timerThread)
    {
        if (player == null || player.TempData.Invincible)
            return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.PoisonDamage, damage, hurtLength, true, hazardId, false));

        player.ApplyCharacterDamage(damage, hazardId, hurtLength, serverRConfig, timerThread);
    }

    public class StatusData()
    {
        public Player Player;
        public ItemEffect Effect;
    }

    public static void TemporaryStatus(this Player player, ItemEffect effect, TimerThread timerThread)
    {
        player.TempData.AddStatusEffect(effect);

        var statusData = new StatusData { Player = player, Effect = effect };

        timerThread.DelayCall(DisableStatus, statusData,
            TimeSpan.FromSeconds(effect.Duration), TimeSpan.Zero, 1);
    }

    public static void DisableStatus(object data)
    {
        var statusData = (StatusData)data;
        statusData.Player.TempData.RemoveStatusEffect(statusData.Effect);
    }

    public class InvincibilityData()
    {
        public Player Player;
        public bool IsInvincible;
    }

    public static void TemporaryInvincibility(this Player player, TimerThread timerThread, double durationInSeconds)
    {
        player.TempData.Invincible = true;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                 (int)ItemEffectType.Invincibility, 0, (int)durationInSeconds, true, player.CharacterName, true));

        var invincibleData = new InvincibilityData()
        {
            Player = player,
            IsInvincible = false
        };

        timerThread.DelayCall(DisableInvincibility, invincibleData, TimeSpan.FromSeconds(durationInSeconds), TimeSpan.Zero, 1);
    }

    public static void DisableInvincibility(object data)
    {
        var invincibleData = (InvincibilityData)data;
        invincibleData.Player.TempData.Invincible = false;
    }
}
