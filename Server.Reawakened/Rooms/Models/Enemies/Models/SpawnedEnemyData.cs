using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Components.GameObjects.Controllers;
using Server.Reawakened.Entities.Components.GameObjects.Hazards;
using Server.Reawakened.Entities.Components.Misc;

namespace Server.Reawakened.Entities.Enemies.Models;
public class SpawnedEnemyData(AIStatsGlobalComp global, AIStatsGenericComp generic, InterObjStatusComp status,
    EnemyControllerComp enemyController, HazardControllerComp hazard, GlobalProperties globalProperties)
{
    public AIStatsGlobalComp Global => global;
    public AIStatsGenericComp Generic => generic;
    public InterObjStatusComp Status => status;
    public EnemyControllerComp EnemyController => enemyController;
    public HazardControllerComp Hazard => hazard;
    public GlobalProperties GlobalProperties => globalProperties;
}
