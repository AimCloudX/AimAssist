using AimAssist.Core.Options;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using System.IO;

namespace AimAssist.Unit.Implementation.Options
{
    public class OptionUnitsFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => OptionMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new OptionUnit("Option", "Option");
        }
    }
}
