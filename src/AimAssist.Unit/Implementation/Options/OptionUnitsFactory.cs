using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Options
{
    public class OptionUnitsFactory : IUnitsFacotry
    {
        public IMode TargetMode => OptionMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new OptionUnit("Option");
            yield return new KeyboardShortcutsOptionUnit();
        }
    }
}
