using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Hazards;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs;

namespace Server.Reawakened.Entities.Enemies.Models;
public class SpawnedEnemyData(AIStatsGlobalComp global, AIStatsGenericComp generic, InterObjStatusComp status,
    IEnemyController enemyController, HazardControllerComp hazard, GlobalProperties globalProperties)
{
    public AIStatsGlobalComp Global => global;
    public AIStatsGenericComp Generic => generic;
    public InterObjStatusComp Status => status;
    public IEnemyController EnemyController => enemyController;
    public HazardControllerComp Hazard => hazard;
    public GlobalProperties GlobalProperties => globalProperties;
}
