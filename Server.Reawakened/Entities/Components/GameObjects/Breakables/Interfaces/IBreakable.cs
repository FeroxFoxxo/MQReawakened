namespace Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;

public interface IBreakable
{
    public int NumberOfHits { get; set; }
    public int NumberOfHitsToBreak { get; }
}
