using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Options
{
    public class OptionUnitsFactory : IUnitsFacotry
    {
        public IMode TargetMode => OptionMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            yield return new OptionUnit();
        }
    }
    public class ShortcutUnitsFacotry  : IUnitsFacotry
    {
        public IMode TargetMode => OptionMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            yield return new ShortcutOptionUnit();
        }
    }
}
