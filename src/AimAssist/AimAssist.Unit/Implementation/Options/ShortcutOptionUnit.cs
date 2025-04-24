using AimAssist.Core.Units;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Options
{
    public class ShortcutOptionUnit : IUnit
    {
        public IMode Mode => OptionMode.Instance;

        public string Name => "Shortcuts Settings";

        public string Description => string.Empty;

        public string Category => string.Empty;
    }
}
