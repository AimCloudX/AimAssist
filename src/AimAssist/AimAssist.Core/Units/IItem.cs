namespace AimAssist.Core.Units;

public interface IItem
{
        IMode Mode { get; }
        string Name { get; }
        string Description { get; }
        string Category { get; }
}