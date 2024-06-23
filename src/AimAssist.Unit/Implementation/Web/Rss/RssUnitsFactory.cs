using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Web.Rss
{
    public class RssUnitsFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => RssMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new RssUnit();
        }
    }
}
