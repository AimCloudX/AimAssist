using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Core
{
    public interface IUnitsFacotry
    {
        IMode TargetMode { get; }
        IAsyncEnumerable<IUnit> GetUnits();
        bool IsShowInStnadard { get; }
    }
}
