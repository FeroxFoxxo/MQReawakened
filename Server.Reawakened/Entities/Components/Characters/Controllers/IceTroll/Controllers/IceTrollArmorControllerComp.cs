using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.Controllers;
public class IceTrollArmorControllerComp : Component<IceTrollArmorController>
{
    public float DamageRatio => ComponentData.DamageRatio;
    public float HealthRatio => ComponentData.HealthRatio;
    public Vector2 ArmorSize => ComponentData.ArmorSize;
    public Vector2 ArmorOffset => ComponentData.ArmorOffset;
}
