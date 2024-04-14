using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.GameObjects.Healing;
public class HealerControllerComp : Component<HealerController>, IDestructible
{
    public float HealFrequency => ComponentData.HealFrequency;
    public int HealPoints => ComponentData.HealPoints;
    public string HealTargetId => ComponentData.HealTargetId;

    private bool _destroyed;
    private float _lastUpdate;
    private BaseEnemy _enemy;

    public override void InitializeComponent()
    {
        _destroyed = false;
        _lastUpdate = Room.Time;
        _enemy = Room.GetEntityFromId<BaseEnemy>(HealTargetId);
    }

    public override object[] GetInitData(Player player) => [_destroyed ? 0 : 1];

    public void Destroy(Room room, string id) => _destroyed = true;

    public override void Update()
    {
        if (_lastUpdate + HealFrequency < Room.Time)
        {
            _lastUpdate = Room.Time;
            _enemy.Heal(HealPoints);
        }
    }
}
