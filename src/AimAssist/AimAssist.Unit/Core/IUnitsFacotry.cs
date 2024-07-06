using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Core
{
    public interface IUnitsFacotry
    {
        IEnumerable<IUnit> GetUnits();
    }
}
