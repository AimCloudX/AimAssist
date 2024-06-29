using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using Library.Rss;

namespace AimAssist.Units.Implementation.Web.Rss
{
    public class RssUnitsFactory : IUnitsFacotry
    {
        public IMode TargetMode => RssMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            yield return new Unit(TargetMode, "Rss Setting", new RssSetting());
        }
    }

    public class RssSetting : IUnitContent
    {
    }
}
