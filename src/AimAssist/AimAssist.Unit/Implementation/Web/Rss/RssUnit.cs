using AimAssist.Core.Units;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Web.Rss
{
    public class RssSettingUnit : IUnit
    {
        public IMode Mode => RssMode.Instance;

        public string Name => "Rss Setting";

        public string Description => string.Empty;

        public string Category => string.Empty;
    }
}
