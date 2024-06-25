using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Standard;

namespace AimAssist.Unit.Implementation.Commands
{
    public class AppCommandUnitFactory : IUnitsFacotry
    {
        public IMode TargetMode => AllInclusiveMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new AppCommandUnit();
        }
    }
}
