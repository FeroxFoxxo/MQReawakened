using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.Controllers;
public class IceTrollStalactiteControllerComp : Component<IceTrollStalactiteController>
{
    public int[] Stalactites => ComponentData.Stalactites;
    public string[] PatternsPhase1 => ComponentData.PatternsPhase1;
    public string[] PatternsPhase2 => ComponentData.PatternsPhase2;
    public string[] PatternsPhase3 => ComponentData.PatternsPhase3;
}
