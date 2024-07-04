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
            yield return new RssSettingUnit();
        }
    }

    public class RssSettingUnit : IUnit
    {
        public IMode Mode => RssMode.Instance;

        public string Name => "Rss Setting";

        public string Description => string.Empty;

        public string Category => string.Empty;
    }
}
