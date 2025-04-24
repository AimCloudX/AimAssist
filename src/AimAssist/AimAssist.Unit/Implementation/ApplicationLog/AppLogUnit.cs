using AimAssist.Core.Units;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.ApplicationLog
{
    public class AppLogUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "Application Log";

        public string Description => string.Empty;

        public string Category => string.Empty;
    }
}
