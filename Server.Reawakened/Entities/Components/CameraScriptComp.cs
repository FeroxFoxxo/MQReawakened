using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class CameraScriptComp : Component<CameraScript>
{
    public bool DisablePlayerInput => ComponentData.DisablePlayerInput;
    public bool TriggerIfInBox => ComponentData.TriggerIfInBox;
    public bool TriggerOnlyOnce => ComponentData.TriggerOnlyOnce;
    public bool HideHud => ComponentData.HideHud;
    public float CamZoneDuration1 => ComponentData.CamZoneDuration1;
    public float CamZoneDuration2 => ComponentData.CamZoneDuration2;
    public float CamZoneDuration3 => ComponentData.CamZoneDuration3;
    public float CamZoneDuration4 => ComponentData.CamZoneDuration4;
    public float CamZoneDuration5 => ComponentData.CamZoneDuration5;
    public float CamZoneDuration6 => ComponentData.CamZoneDuration6;
    public float CamZoneDuration7 => ComponentData.CamZoneDuration7;
    public float CamZoneDuration8 => ComponentData.CamZoneDuration8;
    public float CamZoneDuration9 => ComponentData.CamZoneDuration9;
    public float CamZoneDuration10 => ComponentData.CamZoneDuration10;
    public float CamZoneDuration11 => ComponentData.CamZoneDuration11;
    public float CamZoneDuration12 => ComponentData.CamZoneDuration12;
    public float CamZoneDuration13 => ComponentData.CamZoneDuration13;
    public float CamZoneDuration14 => ComponentData.CamZoneDuration14;
    public float CamZoneDuration15 => ComponentData.CamZoneDuration15;
    public string CamZone1 => ComponentData.CamZone1;
    public string CamZone2 => ComponentData.CamZone2;
    public string CamZone3 => ComponentData.CamZone3;
    public string CamZone4 => ComponentData.CamZone4;
    public string CamZone5 => ComponentData.CamZone5;
    public string CamZone6 => ComponentData.CamZone6;
    public string CamZone7 => ComponentData.CamZone7;
    public string CamZone8 => ComponentData.CamZone8;
    public string CamZone9 => ComponentData.CamZone9;
    public string CamZone10 => ComponentData.CamZone10;
    public string CamZone11 => ComponentData.CamZone11;
    public string CamZone12 => ComponentData.CamZone12;
    public string CamZone13 => ComponentData.CamZone13;
    public string CamZone14 => ComponentData.CamZone14;
    public string CamZone15 => ComponentData.CamZone15;
    public float DelayBeforeScriptStart => ComponentData.DelayBeforeScriptStart;
    public string TriggerZoneId => ComponentData.TriggerZoneId;

    public float TotalTime;

    public override void InitializeComponent() => TotalTime =
        CamZoneDuration1 +
        CamZoneDuration2 +
        CamZoneDuration3 +
        CamZoneDuration4 +
        CamZoneDuration5 +
        CamZoneDuration6 +
        CamZoneDuration7 +
        CamZoneDuration8 +
        CamZoneDuration9 +
        CamZoneDuration10 +
        CamZoneDuration11 +
        CamZoneDuration12 +
        CamZoneDuration13 +
        CamZoneDuration14 +
        CamZoneDuration15;

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player) { }
}
