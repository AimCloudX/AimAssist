using AimAssist.Core;
using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Options
{
    [AutoRegisterUnit("Settings", Priority = 60)]
    public class ShortcutOptionUnit : IUnit
    {
        public IMode Mode => OptionMode.Instance;

        public string Name => "Shortcuts Settings";

        public string Description => string.Empty;

        public string Category => Constants.AppName;
    }
}
