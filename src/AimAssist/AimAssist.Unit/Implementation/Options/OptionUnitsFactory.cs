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
            yield return new Unit(TargetMode, "Option Settings", new OptionContent());
        }
    }
    public class ShortcutUnitsFacotry  : IUnitsFacotry
    {
        public IMode TargetMode => OptionMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            yield return new Unit(TargetMode, "Shortcut Settings", new ShortcutOptionContent());
        }
    }
}
