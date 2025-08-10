using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.Computer
{
    [AutoRegisterUnit("System")]
    public class ComputerUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "PC情報";

        public string Description => "PC情報";

        public string Category => string.Empty;
    }
}
