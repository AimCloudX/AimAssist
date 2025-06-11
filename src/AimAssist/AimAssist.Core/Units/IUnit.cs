
namespace AimAssist.Core.Units
{
    public interface IUnit
    {
        IMode Mode { get; }
        string Name { get; }
        string Description { get; }
        string Category { get; }
    }
}
