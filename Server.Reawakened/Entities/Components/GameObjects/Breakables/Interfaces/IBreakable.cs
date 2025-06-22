namespace Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;

public interface IBreakable
{
    int NumberOfHits { get; set; }
    int NumberOfHitsToBreak { get; }
}
