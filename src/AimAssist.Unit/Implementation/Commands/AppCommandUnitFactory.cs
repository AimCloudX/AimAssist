using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Standard;

namespace AimAssist.Unit.Implementation.Commands
{
    public class AppCommandUnitFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => StandardMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new AppCommandUnit();
        }
    }
}
