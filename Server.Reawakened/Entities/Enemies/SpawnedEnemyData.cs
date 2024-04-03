using Server.Reawakened.Entities.Components;

namespace Server.Reawakened.Entities.Enemies;
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
